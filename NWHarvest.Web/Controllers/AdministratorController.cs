using NWHarvest.Web.Enums;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Collections.Generic;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private const int DAY_LIMIT_FOR_ADMINISTRATORS = 180;

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var today = DateTime.UtcNow;
            var vm = new AdministratorViewModel();
            vm.NumberOfGrowers = db.Growers.Count();
            vm.NumberOfFoodBanks = db.FoodBanks.Count();
            vm.NumberOfListings = db.Listings.Count();
            vm.NumberOfAvailableListings = db.Listings.Where(l => l.IsAvailable == true && l.ExpirationDate >= today).Count();
            vm.NumberOfPendingPickupClaimListings = db.Listings.Where(l => l.IsPickedUp == false && l.IsAvailable == false && l.ExpirationDate >= today).Count();
            vm.NumberOfClaimedListings = db.Listings.Where(l => l.IsPickedUp == true).Count();
            vm.NumberOfUnavailableListings = db.Listings
                .Where(l => (l.IsAvailable == false && l.IsPickedUp == false && l.ExpirationDate < today) ||
                    (l.IsAvailable == true && l.ExpirationDate < today))
                .Count();

            return View(vm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageUser(UserRole userRole)
        {
            switch (userRole)
            {
                case UserRole.Administrator:
                    break;
                case UserRole.Grower:
                    return RedirectToAction(nameof(ManageGrowers));
                case UserRole.FoodBank:
                    return RedirectToAction(nameof(ManageFoodBanks));
                default:
                    break;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageFoodBanks()
        {
            var vm = db.FoodBanks
                .OrderBy(g => g.name)
                .Select(g => new FoodBankViewModel
                {
                    Id = g.Id,
                    Name = g.name,
                    IsActive = g.IsActive,
                    CreatedOn = g.CreatedOn,
                    Address = new AddressViewModel {
                        City = g.city,
                        Zip = g.zip
                    }
                }).ToList();

            return View(vm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageFoodBank(int id)
        {
            var foodbank = db.FoodBanks
                .Where(fb => fb.Id == id)
                .Select(fb => new FoodBankViewModel
                {
                    Id = fb.Id,
                    Name = fb.name,
                    Email = fb.email,
                    IsActive = fb.IsActive,
                    Address = new AddressViewModel
                    {
                        Address1 = fb.address1,
                        Address2 = fb.address2,
                        City = fb.city,
                        State = fb.state,
                        Zip = fb.zip
                    }
                }).FirstOrDefault();

            if (foodbank == null)
            {
                return HttpNotFound();
            }

            return View(foodbank);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageGrowers()
        {
            var vm = db.Growers
                .OrderBy(g => g.name)
                .Select(g => new GrowerViewModel
                {
                    Id = g.Id,
                    Name = g.name,
                    CreatedOn = g.CreatedOn,
                    Address = new AddressViewModel
                    {
                        City = g.city,
                        Zip = g.zip
                    },
                    IsActive = g.IsActive
                }).ToList();

            return View(vm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageGrower(int id)
        {
            var grower = db.Growers
                .Where(g => g.Id == id)
                .Select(g => new GrowerViewModel
                {
                    Id = g.Id,
                    Name = g.name,
                    Email = g.email,
                    IsActive = g.IsActive,
                    Address = new AddressViewModel
                    {
                        Address1 = g.address1,
                        Address2 = g.address2,
                        City = g.city,
                        State = g.state,
                        Zip = g.zip
                    }
                }).FirstOrDefault();

            if (grower == null)
            {
                return HttpNotFound();
            }

            return View(grower);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ToggleEnableDisableUser(UserRole userRole, int userId)
        {
            switch (userRole)
            {
                case UserRole.Grower:
                    var grower = db.Growers.Find(userId);
                    if (grower != null)
                    {
                        grower.IsActive = !grower.IsActive;
                        db.SaveChanges();
                    }
                    break;
                case UserRole.FoodBank:
                    var foodbank = db.FoodBanks.Find(userId);
                    if (foodbank != null)
                    {
                        foodbank.IsActive = !foodbank.IsActive;
                        db.SaveChanges();
                    }
                    break;
                default:
                    break;
            }

            return RedirectToAction(nameof(ManageUser), new { UserRole = userRole });
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageListings(ListingStatus listingStatus)
        {
            var today = DateTime.UtcNow;
            var query = db.Listings.AsQueryable();
            
            switch (listingStatus)
            {
                case ListingStatus.Available:
                    ViewBag.PanelHeader = "Available Listings";
                    ViewBag.ListingPartialView = "_AvailableListings";
                    query = query.Where(l => l.IsAvailable == true && l.ExpirationDate >= today);
                    break;
                case ListingStatus.Pickup:
                    ViewBag.PanelHeader = "Pending Pickup Listings";
                    ViewBag.ListingPartialView = "_PendingPickupListings";
                    query = query.Where(l => l.IsAvailable == false && l.IsPickedUp == false && l.ExpirationDate >= today);
                    break;
                case ListingStatus.Claimed:
                    ViewBag.PanelHeader = "Claimed Listings";
                    ViewBag.ListingPartialView = "_ClaimedListings";
                    query = query.Where(l => l.IsPickedUp == true);
                    break;
                case ListingStatus.Expired:
                    ViewBag.PanelHeader = "Expired Available Listings";
                    ViewBag.ListingPartialView = "_ExpiredListings";
                    query = query.Where(l => l.IsAvailable == true && l.ExpirationDate < today);
                    break;
                case ListingStatus.Unavailable:
                    ViewBag.PanelHeader = "Unavailable Listings";
                    ViewBag.ListingPartialView = "_UnavailableListings";
                    query = query.Where(l => (l.IsAvailable == false && l.IsPickedUp == false && l.ExpirationDate < today) ||
                                                (l.IsAvailable == true && l.ExpirationDate < today));

                    break;
                default:
                    return HttpNotFound();
            }

            ViewBag.ReturnUrl = Url.Action(nameof(ManageListings), "Administrator", new { ListingStatus = listingStatus });
            var listings = query.Select(l => new ListingViewModel
            {
                Id = l.Id,
                GrowerName = l.Grower.name,
                FoodBank = new FoodBankViewModel
                {
                    Name = l.FoodBank.name
                },
                Product = l.Product,
                QuantityAvailable = l.QuantityAvailable,
                UnitOfMeasure = l.UnitOfMeasure,
                ExpirationDate = l.ExpirationDate
            }).ToList();

            return View(listings);
        }
    }
}