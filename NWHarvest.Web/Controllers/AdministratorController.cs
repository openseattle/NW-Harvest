using NWHarvest.Web.Enums;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System;

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
                    IsActive = g.IsActive
                }).ToList();

            return View(vm);
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
                    IsActive = g.IsActive
                }).ToList();

            return View(vm);
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
    }
}