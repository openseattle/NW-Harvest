﻿using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NWHarvest.Web.Models;
using NWHarvest.Web.Enums;
using NWHarvest.Web.Helper;
using System;
using System.Security.Principal;
using NWHarvest.Web.ViewModels;
using System.Collections.Generic;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string loginType)
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, string loginType)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToRole(UserManager.FindByEmail(model.Email));
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    var user = UserManager.FindByEmail(model.Email);
                    if (user != null)
                    {
                        if (!user.EmailConfirmed)
                        {
                            await SendEmailConfirmationToken(user.Id);
                            return RedirectToAction(nameof(ConfirmEmail), new { resend = true });
                        }
                    }
                    return View(model);
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        private ActionResult RedirectToRole(ApplicationUser user)
        {
            if (user == null)
            {
                return HttpNotFound();
            }

            // redirect users with no roles to home
            if (user.Roles.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // authorize authenticated user
            var userRoles = UserManager.GetRoles(user.Id);
            if (HttpContext.User == null)
            {
                var identity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                HttpContext.User = new GenericPrincipal(identity, userRoles.ToArray());
            }

            // default to first user role
            switch (Enum.Parse(typeof(UserRole), userRoles.First()))
            {
                case UserRole.Administrator:
                    Session["HomeProfileUrl"] = Url.Action("Profile", "Administrator");
                    return RedirectToAction("Index", "Administrator");
                case UserRole.Grower:
                    Session["HomeProfileUrl"] = Url.Action("Profile", "Growers");
            return RedirectToAction("Profile", "Growers");
                case UserRole.FoodBank:
                    Session["HomeProfileUrl"] = Url.Action("Profile", "FoodBanks");
                    return RedirectToAction("Profile", "FoodBanks");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        private async Task SendEmailConfirmationToken(string userId)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userId, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userId, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult RegisterRouteView()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterViewBag();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    switch (model.UserRole)
                    {
                        case UserRole.FoodBank:
                            return await RegisterFoodBank(model, user);
                        case UserRole.Grower:
                            return await RegisterGrower(model, user);
                        default:
                            UserManager.Delete(user);
                            return View("Error");
                    }
                }
                AddErrors(result);
            }

            RegisterViewBag();
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code, bool resend = false, bool registration = false)
        {
            ViewBag.Confirmed = false;

            if (registration || resend)
            {
                return View();
            }

            if (userId == null || code == null)
            {
                return View("Error");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                ViewBag.Confirmed = true;
                return View();
            }
            else
            {
                return View("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return View("ForgotPasswordUserDoesNotExist");
                }

                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    //SendConfirmationEmail(user);
                    string confirmationCode = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var confirmatonCallbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = confirmationCode }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + confirmatonCallbackUrl + "\">here</a>");
                    return View("ForgotPasswordWithUnconfirmedEmail");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        private void RegisterViewBag()
        {
            // exclude administrator role
            ViewBag.UserRoles = Enum.GetValues(typeof(UserRole))
                             .Cast<UserRole>()
                             .Where(e => e != UserRole.Administrator)
                             .Select(e => new SelectListItem
                             {
                                 Value = ((int)e).ToString(),
                                 Text = e.ToString()
                             });

            ViewBag.Counties = WashingtonState.GetCounties()
                .Select(county => new SelectListItem
                {
                    Text = county,
                    Value = county,
                    Selected = false
                }).ToList();
        }

        private async Task<ActionResult> RegisterFoodBank(RegisterViewModel model, ApplicationUser user)
        {
            try
            {
                var foodbank = new FoodBank()
                {
                    UserId = user.Id,
                    name = model.Name,
                    email = model.Email,
                    address1 = model.Address1,
                    address2 = model.Address2 == null ? "" : model.Address2,
                    city = model.City,
                    county = model.County,
                    state = "WA",
                    zip = model.Zip,
                    NotificationPreference = model.Notification.ToString(),
                    IsActive = true,
                    PickupLocations = new List<PickupLocation>
                    {
                        new PickupLocation
                        {
                            name = "Default",
                            address1 = model.Address1,
                            address2 = model.Address2 == null ? "" : model.Address2,
                            city = model.City,
                            county = model.County,
                            state = "WA",
                            zip = model.Zip
                        }
                    }
                };

                db.FoodBanks.Add(foodbank);
                db.SaveChanges();
                await SendEmailConfirmationToken(user.Id);
                UserManager.AddToRole(user.Id, UserRole.FoodBank.ToString());
                return RedirectToAction(nameof(ConfirmEmail), new { Registration = true });
            }
            catch (Exception)
            {
                UserManager.Delete(user);
                return View("Error");
            }
        }

        private async Task<ActionResult> RegisterGrower(RegisterViewModel model, ApplicationUser user)
        {
            try
            {
                var grower = new Grower()
                {
                    UserId = user.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Address1 = model.Address1,
                    Address2 = model.Address2 == null ? "" : model.Address2,
                    City = model.City,
                    County = model.County,
                    State = "WA",
                    Zip = model.Zip,
                    NotificationPreference = model.Notification.ToString(),
                    IsActive = true,
                    PickupLocations = new List<PickupLocation>
                    {
                        new PickupLocation
                        {
                            name = "Default",
                            address1 = model.Address1,
                            address2 = model.Address2 == null ? "" : model.Address2,
                            city = model.City,
                            county = model.County,
                            state = "WA",
                            zip = model.Zip
                        }
                    }
                };

                db.Growers.Add(grower);
                db.SaveChanges();
                UserManager.AddToRole(user.Id, UserRole.Grower.ToString());
                await SendEmailConfirmationToken(user.Id);
                return RedirectToAction(nameof(ConfirmEmail), new { Registration = true });
            }
            catch (Exception)
            {
                UserManager.Delete(user);
                return View("Error");
            }
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}