using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NWHarvest.Web.Helper;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.ViewModels;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
//using System.Web.Http;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class FoodBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

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
        public ActionResult Register(FoodBank foodbank)
        {
            if (ModelState.IsValid)
            {
                db.FoodBanks.Add(foodbank);
                db.SaveChanges();
                return RedirectToAction("ConfirmEmail", "Account", new { Registration = true });
            }

            return RedirectToAction("Register", "Account");
        }

        [Authorize(Roles = "FoodBank")]
        [ActionName("Profile")]
        public ActionResult Index()
        {
            var vm = db.FoodBanks
                .Where(fb => fb.UserId == UserId)
                .Select(fb => new FoodBankViewModel
                {
                    Id = fb.Id,
                    Name = fb.name,
                    Email = fb.email,
                    NotificationPreference = fb.NotificationPreference,
                    IsActive = fb.IsActive,
                    Address = new AddressViewModel
                    {
                        Address1 = fb.address1,
                        Address2 = fb.address2,
                        Address3 = fb.address3,
                        Address4 = fb.address4,
                        City = fb.city,
                        County = fb.county,
                        State = fb.state,
                        Zip = fb.zip
                    }
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            if (!vm.IsActive)
            {
                return View("DisabledUser");
            }

            // available grower listings
            vm.AvailableListings = db.Listings
                .Where(l => l.IsAvailable == true)
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    ExpirationDate = l.ExpirationDate,
                    Comments = l.Comments,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    }
                }).ToList();

            // claimed listings for pickup
            vm.ClaimedListings = db.Listings
                .Where(l => l.FoodBank.Id == vm.Id && l.IsPickedUp == false)
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Address = new AddressViewModel
                        {
                            Address1 = l.PickupLocation.address1,
                            Address2 = l.PickupLocation.address2,
                            Address3 = l.PickupLocation.address3,
                            Address4 = l.PickupLocation.address4,
                            City = l.PickupLocation.city,
                            County = l.PickupLocation.county,
                            State = l.PickupLocation.state,
                            Zip = l.PickupLocation.zip
                        }
                    },
                    QuantityAvailable = l.QuantityAvailable,
                    Comments = l.Comments,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    }
                }).ToList();

            vm.Listings = db.Listings
                .Select(l => new ViewModels.ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    Comments = l.Comments,
                    IsAvailable = l.IsAvailable,
                    IsPickedUp = l.IsPickedUp,
                    QuantityClaimed = l.QuantityClaimed,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Name = l.PickupLocation.name,
                        Address = new AddressViewModel
                        {
                            Address1 = l.PickupLocation.address1,
                            Address2 = l.PickupLocation.address2,
                            Address3 = l.PickupLocation.address3,
                            Address4 = l.PickupLocation.address4,
                            City = l.PickupLocation.city,
                            County = l.PickupLocation.county,
                            State = l.PickupLocation.state,
                            Zip = l.PickupLocation.zip
                        }
                    },
                    Grower = new GrowerViewModel
                    {
                        IsActive = l.Grower.IsActive
                    },
                    FoodBank = new FoodBankViewModel
                    {
                        UserId = l.FoodBank.UserId
                    },
                }).ToList();

            if (vm == null)
            {
                return HttpNotFound();
            }

            RegisterViewData();
            return View(vm);
        }

        [Authorize(Roles = "FoodBank")]
        public ActionResult Edit(int id)
        {
            var vm = db.FoodBanks
                .Where(fb => fb.Id == id && fb.UserId == UserId)
                .Select(fb => new FoodBankEditViewModel
                {
                    Id = fb.Id,
                    Name = fb.name,
                    Address = new AddressEditViewModel
                    {
                        Address1 = fb.address1,
                        Address2 = fb.address2,
                        Address3 = fb.address3,
                        Address4 = fb.address4,
                        City = fb.city,
                        County = fb.county,
                        State = fb.state,
                        Zip = fb.zip
                    },
                    IsActive = fb.IsActive
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            RegisterViewData();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FoodBank")]
        public ActionResult Edit(FoodBankEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var foodBank = db.FoodBanks.Find(vm.Id);

                if (foodBank == null)
                {
                    return HttpNotFound();
                }

                foodBank.name = vm.Name;
                foodBank.address1 = vm.Address.Address1;
                foodBank.address2 = vm.Address.Address2;
                foodBank.address3 = vm.Address.Address3;
                foodBank.address4 = vm.Address.Address4;
                foodBank.city = vm.Address.City;
                foodBank.county = vm.Address.County;
                foodBank.state = vm.Address.State;
                foodBank.zip = vm.Address.Zip;
                foodBank.IsActive = vm.IsActive;

                db.SaveChanges();
                return RedirectToAction(nameof(Profile));
            }

            RegisterViewData();
            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "FoodBank")]
        public ActionResult Claim(int ListingId)
        {
            var vm = GetClaimListingViewModel(ListingId);

            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "FoodBank")]
        [ValidateAntiForgeryToken]
        public ActionResult Claim([Bind(Include = "Quantity,ListingId")]ClaimListingViewModel claim)
        {
            if (!ModelState.IsValid)
            {
                return View(GetClaimListingViewModel(claim.ListingId));
            }

            var vm = db.Listings
                .Where(l => l.Id == claim.ListingId)
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    ExpirationDate = l.ExpirationDate,
                    Comments = l.Comments,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        UserId = l.Grower.UserId,
                        Email = l.Grower.email,
                        NotificationPreference = l.Grower.NotificationPreference
                    },
                    PickupLocation = new PickupLocationViewModel
                    {
                        Address = new AddressViewModel
                        {
                            Address1 = l.PickupLocation.address1,
                            Address2 = l.PickupLocation.address2,
                            City = l.PickupLocation.city,
                            State = l.PickupLocation.state,
                            County = l.PickupLocation.county,
                            Zip = l.PickupLocation.zip
                        }
                    }
                })
                .AsNoTracking()
                .FirstOrDefault();

            if (claim.Quantity <= 0 || claim.Quantity > vm.QuantityAvailable)
            {
                ModelState.AddModelError("RangeException", "Invalid quantity.");
                return View(GetClaimListingViewModel(claim.ListingId));
            }

            if (vm == null)
            {
                return HttpNotFound();
            }

            var listing = db.Listings.Find(vm.Id);
            var foodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault();
            if (listing == null || foodBank == null)
            {
                return HttpNotFound();
            }

            var foodbankClaim = new FoodBankClaim
            {
                Product = vm.Product,
                Quantity = (int)claim.Quantity,
                CostPerUnit = vm.CostPerUnit,
                Address = new Address
                {
                    Address1 = vm.PickupLocation.Address.Address1,
                    Address2 = vm.PickupLocation.Address.Address2,
                    City = vm.PickupLocation.Address.City,
                    State = vm.PickupLocation.Address.State,
                    County = vm.PickupLocation.Address.County,
                    Zip = vm.PickupLocation.Address.Zip
                },
                FoodBank = foodBank,
                GrowerId = vm.Grower.Id
            };

            db.FoodBankClaims.Add(foodbankClaim);
            if (decimal.Compare(listing.QuantityAvailable, claim.Quantity) == 0)
            {
                db.Listings.Remove(listing);
            } else {
                listing.QuantityAvailable = listing.QuantityAvailable - claim.Quantity;
            }
            
            db.SaveChanges();

            SendNotification(vm);

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize(Roles = "FoodBank")]
        public ActionResult Pickup(int listingId)
        {
            var vm = db.Listings
                .Where(l => l.FoodBank.UserId == UserId && l.Id == listingId)
                .Select(l => new PickupLocationViewModel
                {
                    Id = l.PickupLocation.id,
                    Name = l.PickupLocation.name,
                    Comments = l.PickupLocation.comments,
                    Grower = new GrowerViewModel
                    {
                        Name = l.Grower.name
                    },
                    Listing = new ListingViewModel
                    {
                        Id = l.Id,
                        Product = l.Product,
                        QuantityAvailable = l.QuantityAvailable,
                        UnitOfMeasure = l.UnitOfMeasure
                    },
                    Address = new AddressViewModel
                    {
                        Address1 = l.PickupLocation.address1,
                        Address2 = l.PickupLocation.address2,
                        Address3 = l.PickupLocation.address3,
                        Address4 = l.PickupLocation.address4,
                        City = l.PickupLocation.city,
                        County = l.PickupLocation.county,
                        State = l.PickupLocation.state,
                        Zip = l.PickupLocation.zip
                    }
                }).FirstOrDefault();

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "FoodBank")]
        [ValidateAntiForgeryToken]
        [ActionName("Pickup")]
        public ActionResult PickupConfirm(int listingId)
        {
            Listing listingToUpdate = db.Listings.Find(listingId);
            if (listingToUpdate == null)
            {
                return HttpNotFound();
            }

            listingToUpdate.IsPickedUp = true;
            db.SaveChanges();

            return RedirectToAction(nameof(Profile));
        }

        public ActionResult Settings()
        {
            return RedirectToAction("Index", "Manage");
        }

        private string UserId => User.Identity.GetUserId();

        // send sms and or email to grower
        private void SendNotification(ListingViewModel vm)
        {
            var growerPhoneNumber = UserManager.GetPhoneNumber(vm.Grower.UserId);
            var foodBankPhoneNumber = UserManager.GetPhoneNumber(UserId);
            var foodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault();

            // todo: implement enum Notifications
            // see Account/Registration for list of valid values
            // both
            // emailNote
            // textNote
            switch (vm.Grower.NotificationPreference.ToString().ToLower())
            {
                case "both":
                    SendSmsNotification(vm.Product, foodBank, foodBankPhoneNumber, growerPhoneNumber);
                    SendEmailNotification(vm.Grower.Email, vm.Product, foodBank, foodBankPhoneNumber);
                    break;
                case "emailnote":
                    SendEmailNotification(vm.Grower.Email, vm.Product, foodBank, foodBankPhoneNumber);
                    break;
                case "textnote":
                    SendSmsNotification(vm.Product, foodBank, foodBankPhoneNumber, growerPhoneNumber);
                    break;
                default:
                    return;
            }
        }

        private void SendSmsNotification(string product, FoodBank foodBank, string foodBankPhoneNumber, string growerPhoneNumber = null)
        {
            // bail grower does not have a phone number
            if (growerPhoneNumber == null)
            {
                return;
            }

            var body = $"Your listing of {product} has been claimed by {foodBank.name}, {foodBank.email}";

            if (foodBankPhoneNumber != null)
            {
                body += ", " + foodBankPhoneNumber;
            }
            var message = new IdentityMessage
            {
                Destination = growerPhoneNumber,
                Subject = "",
                Body = body
            };

            UserManager.SmsService.SendAsync(message).Wait();
        }

        private void SendEmailNotification(string growerEmail, string product, FoodBank foodBank, string foodBankPhoneNumber = null)
        {
            var subject = $"NW Harvest listing of {product} has been claimed by {foodBank.name}";
            var body = $"Your listing of {product} has been claimed by {foodBank.name}, {foodBank.email}";

            if (foodBankPhoneNumber != null)
            {
                body += ", " + foodBankPhoneNumber;
            }
            var message = new IdentityMessage
            {
                Destination = growerEmail,
                Subject = subject,
                Body = body
            };

            UserManager.EmailService.SendAsync(message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Helpers
        private ClaimListingViewModel GetClaimListingViewModel(int listingId)
        {
            return db.Listings
                    .Where(l => l.Id == listingId)
                    .Select(l => new ClaimListingViewModel
                    {
                        ListingId = l.Id,
                        Product = l.Product,
                        Available = l.QuantityAvailable,
                        CostPerUnit = l.CostPerUnit,
                        UnitOfMeasure = l.UnitOfMeasure,
                        GrowerName = l.Grower.name
                    })
                    .AsNoTracking()
                    .FirstOrDefault();
        }
        private void RegisterViewData()
        {
            ViewData["States"] = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Washington",
                    Value = "WA",
                    Selected = true
                }
            };
            
            ViewData["Counties"] = WashingtonState.GetCounties()
                .Select(county => new SelectListItem
                {
                    Text = county,
                    Value = county,
                    Selected = false
                }).ToList();
        }
        #endregion
    }
}
