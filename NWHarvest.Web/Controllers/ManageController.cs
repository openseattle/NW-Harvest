using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NWHarvest.Web.Models;
using NWHarvest.Web.Enums;
using NWHarvest.Web.ViewModels;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _db = new ApplicationDbContext();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.ChangePhoneSuccess ? "Your phone number has been changed."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ChangeNotification ? "Your notification preference has been changed."
                : "";

            var vm = await GetUser();

            UpdateNotification(vm);
            return View(vm);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        public async Task<ActionResult> PhoneNumber()
        {
            var user = await GetUser();
            var model = new PhoneNumberViewModel
            {
                Number = await UserManager.GetPhoneNumberAsync(UserId),
                UserName = user.Name
            };

            if (string.IsNullOrWhiteSpace(model.Number))
            {
                model.Message = ManageMessageId.AddPhoneSuccess;
                model.Action = "Add";
            } else
            {
                model.Message = ManageMessageId.ChangePhoneSuccess;
                model.Action = "Update";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PhoneNumber(PhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // do nothing if phone is the same
            var user = await GetUser();
            if (user.PhoneNumber == model.Number)
            {
                return RedirectToAction(nameof(Index));
            }

            // Don't verify FoodBank phone number, primarily uses landline
            if (UserManager.IsInRole(UserId, UserRole.FoodBank.ToString()))
            {
                var result = await SetPhoneNumber(model.Number);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index), new { Message = model.Message });
                }

                ModelState.AddModelError("", "Unable to add phone number.  Please contact administrator for assistance");
                return View(model);
            }

            // verify phone number of growers
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(UserId, model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }

            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.Number });
        }

        [HttpGet]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var user = await GetUser();
            var model = new UserViewModel
            {
                Name = user.Name,
                PhoneNumber = await UserManager.GetPhoneNumberAsync(UserId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("RemovePhoneNumber")]
        public async Task<ActionResult> RemovePhoneNumberConfirmed()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        public async Task<ActionResult> Notification()
        {
            var user = await GetUser();

            var model = new NotificationViewModel
            {
                UserName = user.Name,
                Method = (UserNotification)Enum.Parse(typeof(UserNotification), user.NotificationPreference),
                Message = ManageMessageId.ChangeNotification
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Notification(NotificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userRoles = await UserManager.GetRolesAsync(UserId);
                if (userRoles.Contains(UserRole.Administrator.ToString()))
                {
                    // todo: implement with administrator profile
                    return RedirectToAction(nameof(Index), new { Message = model.Message });
                }

                if (userRoles.Contains(UserRole.FoodBank.ToString()))
                {
                    var foodbank = _db.FoodBanks.Where(f => f.UserId == UserId).FirstOrDefault();
                    if (foodbank == null)
                    {
                        return View("Error");
                    }

                    foodbank.NotificationPreference = model.Method.ToString();
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index), new { Message = model.Message });
                }

                if (userRoles.Contains(UserRole.Grower.ToString()))
                {
                    var grower = _db.Growers.Where(g => g.UserId == UserId).FirstOrDefault();
                    if (grower == null)
                    {
                        return View("Error");
                    }
                    grower.NotificationPreference = model.Method.ToString();
                    _db.SaveChanges();

                    return RedirectToAction(nameof(Index), new { Message = model.Message });
                }
            }
            
            return View(model);
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null)
            {
                return View("Error");
            }

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(UserId, phoneNumber);
            var user = await GetUser();
            var model = new VerifyPhoneNumberViewModel
            {
                UserName = user.Name,
                PhoneNumber = phoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(UserId, model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone.  The code entered is not valid.");
            return View(model);
        }
        
        public async Task<ActionResult> ChangePassword()
        {
            var user = await GetUser();
            var model = new ChangePasswordViewModel
            {
                UserName = user.Name
            };

            return View(model);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers


        // returns user role with highest privileges
        private async Task<UserViewModel> GetUser()
        {
            var userRoles = UserManager.GetRoles(UserId);
            UserViewModel vm;
            if (userRoles.Contains(UserRole.Administrator.ToString()))
            {
                return new UserViewModel
                {
                    Name = UserRole.Administrator.ToString(),
                    ProfileUrl = Url.Action("Profile", "Administrators"),
                    PhoneNumber = await UserManager.GetPhoneNumberAsync(UserId),
                    HasPassword = HasPassword()
                };
            }

            if (userRoles.Contains(UserRole.FoodBank.ToString()))
            {

                vm = _db.FoodBanks
                    .Where(f => f.UserId == UserId)
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        Name = u.name,
                        NotificationPreference = u.NotificationPreference
                    })
                    .FirstOrDefault();

                vm.ProfileUrl = Url.Action("Profile", "FoodBanks");
                vm.PhoneNumber = await UserManager.GetPhoneNumberAsync(UserId);
                vm.HasPassword = HasPassword();
                return vm;
            }

            if (userRoles.Contains(UserRole.Grower.ToString()))
            {
                vm = _db.Growers
                    .Where(f => f.UserId == UserId)
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        Name = u.name,
                        NotificationPreference = u.NotificationPreference
                    })
                    .FirstOrDefault();
                vm.ProfileUrl = Url.Action("Profile", "Growers");
                vm.PhoneNumber = await UserManager.GetPhoneNumberAsync(UserId);
                vm.HasPassword = HasPassword();
                return vm;
            }

            return new UserViewModel();
        }

        private async Task<IdentityResult> SetPhoneNumber(string phoneNumber)
        {
            return await UserManager.SetPhoneNumberAsync(UserId, phoneNumber);
        }

        // helper method
        // todo: update register page form to use UserNotification enum

        private string UserId => User.Identity.GetUserId();
        private void UpdateNotification(UserViewModel vm)
        {
            switch (vm.NotificationPreference)
            {
                case "emailNote":
                    vm.NotificationPreference = UserNotification.Email.ToString();
                    break;
                case "textNote":
                    vm.NotificationPreference = UserNotification.Text.ToString();
                    break;
                case "both":
                    vm.NotificationPreference = UserNotification.Both.ToString();
                    break;
                default:
                    break;
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

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            ChangeNotification,
            Error
        }

        #endregion
    }
}