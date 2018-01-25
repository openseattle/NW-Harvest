using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NWHarvest.Web.Helper;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.ViewModels;
using System.Data.Entity;
using System;
//using System.Web.Http;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class FoodBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private NotificationManager notificationManager = new NotificationManager();
        
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

            vm.AvailableListings = db.Listings
                .Where(l => l.IsAvailable == true && l.ExpirationDate > DateTime.UtcNow)
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    ExpirationDate = l.ExpirationDate,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    },
                    PickupLocation = new PickupLocationViewModel
                    {
                        Address = new AddressViewModel
                        {
                            City = l.PickupLocation.city,
                            County = l.PickupLocation.county,
                            Zip = l.PickupLocation.zip
                        }
                    },
                }).ToList();

            vm.Claims = db.FoodBankClaims
                .Where(c => c.FoodBankId == vm.Id)
                .Select(c => new ClaimViewModel
                {
                    ListingId = c.ListingId,
                    ClaimedOn = c.CreatedOn,
                    Product = c.Product,
                    Quantity = c.Quantity,
                    CostPerUnit = c.CostPerUnit,
                    Grower = new GrowerViewModel
                    {
                        Id = c.GrowerId,
                        Name = c.Grower.name
                    },
                    Address = new AddressViewModel
                    {
                        Address1 = c.Address.Address1,
                        Address2 = c.Address.Address2,
                        City = c.Address.City,
                        State = c.Address.State,
                        County = c.Address.County,
                        Zip = c.Address.Zip
                    }
                })
                .ToList();

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
                ListingId = listing.Id,
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
            listing.QuantityAvailable = listing.QuantityAvailable - claim.Quantity;
            listing.QuantityClaimed = listing.QuantityClaimed + claim.Quantity;

            db.SaveChanges();

            SendNotification(vm);

            return RedirectToAction(nameof(Profile));
        }

        public ActionResult Settings()
        {
            return RedirectToAction("Index", "Manage");
        }

        private string UserId => User.Identity.GetUserId();

        // send sms and/or email notification to grower
        private void SendNotification(ListingViewModel vm)
        {
            var growerPhoneNumber = notificationManager.GetUserPhoneNumber(vm.Grower.UserId);
            var foodBankPhoneNumber = notificationManager.GetUserPhoneNumber(UserId);
            var foodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault();

            var subject = $"NW Harvest listing of {vm.Product} has been claimed by {foodBank.name}";
            var body = $"Your listing of {vm.Product} has been claimed by {foodBank.name}, {foodBank.email}";

            if (foodBankPhoneNumber != null)
            {
                body += ", " + foodBankPhoneNumber;
            }

            var message = new NotificationMessage
            {
                DestinationPhoneNumber = growerPhoneNumber,
                DestinationEmailAddress = vm.Grower.Email,
                Subject = subject,
                Body = body
            };

            notificationManager.SendNotification(message, vm.Grower.NotificationPreference);
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
