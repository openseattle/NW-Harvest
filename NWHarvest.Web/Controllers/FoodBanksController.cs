using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using NWHarvest.Web.Helper;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.ViewModels;
using System.Data.Entity;
using System;
using NWHarvest.Web.Enums;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class FoodBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private NotificationManager notificationManager = new NotificationManager();
        private string UserId => User.Identity.GetUserId();

        [Authorize(Roles = "FoodBank")]
        [ActionName("Profile")]
        public ActionResult Index()
        {
            var vm = db.FoodBanks
                .Where(fb => fb.UserId == UserId)
                .Select(fb => new FoodBankViewModel
                {
                    Id = fb.Id,
                    Name = fb.Name,
                    Email = fb.Email,
                    NotificationPreference = fb.NotificationPreference,
                    IsActive = fb.IsActive,
                    Address = new AddressViewModel
                    {
                        Address1 = fb.Address1,
                        Address2 = fb.Address2,
                        Address3 = fb.Address3,
                        Address4 = fb.Address4,
                        City = fb.City,
                        County = fb.County,
                        State = fb.State,
                        Zip = fb.Zip
                    }
                }).FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            if (!vm.IsActive)
            {
                return View("DisabledUser");
            }

            var queryMyListings = db.Listings.Where(l => l.ListerUserId == UserId);
            vm.MyListings = GetListings(queryMyListings);

            var queryAvailableListings = db.Listings
                .Where(l => l.ListerUserId != UserId)
                .Where(l => l.IsAvailable == true && l.ExpirationDate > DateTime.UtcNow).AsQueryable();
            vm.AvailableListings = GetListings(queryAvailableListings);

            var queryClaimedListings = db.FoodBankClaims.Where(c => c.FoodBankId == vm.Id);
            vm.Claims = GetClaimedListings(queryClaimedListings);

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
                    Name = fb.Name,
                    Address = new AddressEditViewModel
                    {
                        Address1 = fb.Address1,
                        Address2 = fb.Address2,
                        Address3 = fb.Address3,
                        Address4 = fb.Address4,
                        City = fb.City,
                        County = fb.County,
                        State = fb.State,
                        Zip = fb.Zip
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

                foodBank.Name = vm.Name;
                foodBank.Address1 = vm.Address.Address1;
                foodBank.Address2 = vm.Address.Address2;
                foodBank.Address3 = vm.Address.Address3;
                foodBank.Address4 = vm.Address.Address4;
                foodBank.City = vm.Address.City;
                foodBank.County = vm.Address.County;
                foodBank.State = vm.Address.State;
                foodBank.Zip = vm.Address.Zip;
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

            var queryListing = db.Listings.Where(l => l.Id == claim.ListingId);
            var quantityAvailable = queryListing.Select(l => l.QuantityAvailable).FirstOrDefault();

            if (claim.Quantity <= 0 || claim.Quantity > quantityAvailable)
            {
                ModelState.AddModelError("RangeException", "Invalid quantity.");
                return View(GetClaimListingViewModel(claim.ListingId));
            }

            var listing = queryListing.Include("PickupLocation").FirstOrDefault();
            var foodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault();
            if (listing == null || foodBank == null)
            {
                return HttpNotFound();
            }

            var foodbankClaim = new FoodBankClaim
            {
                ListingId = listing.Id,
                Product = listing.Product,
                Quantity = (int)claim.Quantity,
                CostPerUnit = listing.CostPerUnit,
                Address = new Address
                {
                    Address1 = listing.PickupLocation.address1,
                    Address2 = listing.PickupLocation.address2,
                    City = listing.PickupLocation.city,
                    State = listing.PickupLocation.state,
                    County = listing.PickupLocation.county,
                    Zip = listing.PickupLocation.zip
                },
                FoodBank = foodBank
            };

            db.FoodBankClaims.Add(foodbankClaim);
            listing.QuantityAvailable = listing.QuantityAvailable - claim.Quantity;
            listing.QuantityClaimed = listing.QuantityClaimed + claim.Quantity;

            db.SaveChanges();

            SendNotification(foodbankClaim);

            return RedirectToAction(nameof(Profile));
        }

        public ActionResult Settings()
        {
            return RedirectToAction("Index", "Manage");
        }

        private void SendNotification(FoodBankClaim claim)
        {
            var lister = db.Listings
                .Include("User")
                .Where(l => l.Id == claim.ListingId)
                .Select(l => new {
                    UserId = l.User.Id,
                    Email = l.User.Email,
                    Phone = l.User.PhoneNumber,
                    Role = l.ListerRole
                }).First();

            var foodBankPhoneNumber = notificationManager.GetUserPhoneNumber(UserId);
            var foodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault();
            var subject = $"NW Harvest listing";
            var body = $"Your listing of {claim.Product} has been claimed by {foodBank.Name} ({foodBank.Email})";
            if (foodBankPhoneNumber != null)
            {
                body += ", " + foodBankPhoneNumber;
            }

            var message = new NotificationMessage
            {
                DestinationPhoneNumber = lister.Phone,
                DestinationEmailAddress = lister.Email,
                Subject = subject,
                Body = body
            };

            string listerNotificationPreference = string.Empty;
            switch ((ListerRole)Enum.Parse(typeof(ListerRole), lister.Role))
            {
                case ListerRole.Grower:
                    listerNotificationPreference = db.Growers.Where(g => g.UserId == lister.UserId).First().NotificationPreference;
                    break;
                case ListerRole.FoodBank:
                    listerNotificationPreference = db.FoodBanks.Where(g => g.UserId == lister.UserId).First().NotificationPreference;
                    break;
                default:
                    break;
            }

            notificationManager.SendNotification(message, listerNotificationPreference);
        }
        private ICollection<ClaimViewModel> GetClaimedListings(IQueryable<FoodBankClaim> query)
        {
            return query.Select(c => new ClaimViewModel
            {
                ListingId = c.ListingId,
                ClaimedOn = c.CreatedOn,
                Product = c.Product,
                Quantity = c.Quantity,
                CostPerUnit = c.CostPerUnit,
                Address = new AddressViewModel
                {
                    Address1 = c.Address.Address1,
                    Address2 = c.Address.Address2,
                    City = c.Address.City,
                    State = c.Address.State,
                    County = c.Address.County,
                    Zip = c.Address.Zip
                }
            }).ToList();
        }
        private ICollection<ListingViewModel> GetListings(IQueryable<Listing> query)
        {
            return query
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    ExpirationDate = l.ExpirationDate,
                    HarvestDate = l.HarvestedDate,
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
            var vm = db.Listings
                    .Where(l => l.Id == listingId)
                    .Select(l => new ClaimListingViewModel
                    {
                        ListingId = l.Id,
                        Product = l.Product,
                        Available = l.QuantityAvailable,
                        CostPerUnit = l.CostPerUnit,
                        UnitOfMeasure = l.UnitOfMeasure,
                        ListerUserId = l.ListerUserId,
                        ListerRole = l.ListerRole
                    })
                    .AsNoTracking()
                    .FirstOrDefault();
            vm.Lister = GetLister(vm.ListerRole, vm.ListerUserId);
            return vm;
        }

        private ListerViewModel GetLister(string role, string userId)
        {
            switch ((ListerRole)Enum.Parse(typeof(ListerRole), role))
            {
                case ListerRole.Grower:
                    return db.Growers.Where(g => g.UserId == userId)
                        .Select(g => new ListerViewModel
                        {
                            Id = g.Id,
                            UserId = g.UserId,
                            Name = g.Name,
                            Email = g.Email,
                            NotificationPreference = g.NotificationPreference
                        }).FirstOrDefault();
                case ListerRole.FoodBank:
                    return db.FoodBanks.Where(fb => fb.UserId == userId)
                        .Select(fb => new ListerViewModel
                        {
                            Id = fb.Id,
                            UserId = fb.UserId,
                            Name = fb.Name,
                            Email = fb.Email,
                            NotificationPreference = fb.NotificationPreference
                        }).FirstOrDefault();
                default:
                    throw new NotImplementedException();
            }

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
