using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using System.Collections.Generic;

namespace NWHarvest.Web.Controllers
{
    public class ListingsViewModel
    {
        public RegisteredUser registeredUser { get; set; }
        public IEnumerable<Listing> FirstList { get; set; }
        public IEnumerable<Listing> SecondList { get; set; }
        public IEnumerable<Listing> ThirdList { get; set; }
        public IEnumerable<PickupLocation> PickupLocations { get; set; }
    }

    [Authorize]
    public class ListingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private const int DAY_LIMIT_FOR_GROWERS = 31;
        private const int DAY_LIMIT_FOR_FOOD_BANKS = 31;
        private const int DAY_LIMIT_FOR_ADMINISTRATORS = 180;

        public ActionResult Index()
        {
            var registeredUserService = new RegisteredUserService();
            var user = registeredUserService.GetRegisteredUser(this.User);

            var repo = new ListingsRepository();
            var listingsViewModel = new ListingsViewModel();
            listingsViewModel.registeredUser = user;

            if (user.Role == UserRoles.AdministratorRole)
            {
                listingsViewModel.FirstList = repo.GetAllAvailable();
                listingsViewModel.SecondList = repo.GetAllClaimedNotPickedUp(DAY_LIMIT_FOR_ADMINISTRATORS);
                listingsViewModel.ThirdList = repo.GetAllUnavailableExpired(DAY_LIMIT_FOR_ADMINISTRATORS);
            }

            else if (user.Role == UserRoles.GrowerRole)
            {
                listingsViewModel.FirstList = repo.GetAvailableByGrower(user.GrowerId);
                listingsViewModel.SecondList = repo.GetClaimedNotPickedNotExpiredUpByGrower(user.GrowerId, DAY_LIMIT_FOR_GROWERS);
                listingsViewModel.ThirdList = repo.GetExpiredOrPickedUpByGrower(user.GrowerId, DAY_LIMIT_FOR_GROWERS);
            }

            else if (user.Role == UserRoles.FoodBankRole)
            {
                listingsViewModel.FirstList = repo.GetAllAvailable();
                listingsViewModel.SecondList = repo.GetClaimedNotPickedUpNotExpiredByFoodBank(user.FoodBankId, DAY_LIMIT_FOR_FOOD_BANKS);
                listingsViewModel.ThirdList = repo.GetClaimedPickedUpByFoodBankNotPickedUp(user.FoodBankId, DAY_LIMIT_FOR_FOOD_BANKS);
            }

            return View(listingsViewModel);
        }

        public ActionResult Manage()
        {
            var vm = db.Listings
                .Where(l => l.Grower.UserId == UserId)
                .Include("PickupLocation")
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    QuantityClaimed = l.QuantityClaimed,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    CostPerUnit = l.CostPerUnit,
                    IsAvailable = l.IsAvailable,
                    Comments = l.Comments,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    }
                })
                .ToList();

            ViewBag.GrowerName = db.Growers.Where(g => g.UserId == UserId).FirstOrDefault()?.name;

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Details(int id)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = db.Listings
                .Where(l => l.Grower.UserId == UserId && l.Id == id)
                .Include("Grower")
                .Include("PickupLocation")
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    QuantityClaimed = l.QuantityClaimed,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    CostPerUnit = l.CostPerUnit,
                    IsAvailable = l.IsAvailable,
                    Comments = l.Comments,
                    GrowerName = l.Grower.name,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Id = l.PickupLocation.id,
                        Name = l.PickupLocation.name,
                        Address = new AddressViewModel
                        {
                            Address1 = l.PickupLocation.address1,
                            Address2 = l.PickupLocation.address2,
                            Address3 = l.PickupLocation.address3,
                            Address4 = l.PickupLocation.address4,
                            City = l.PickupLocation.city,
                            State = l.PickupLocation.state,
                            Zip = l.PickupLocation.zip
                        }
                    }
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Create()
        {
            var vm = new ListingViewModel
            {
                PickupLocations = SelectListPickupLocations()
            };

            ViewBag.GrowerName = db.Growers.Where(g => g.UserId == UserId).FirstOrDefault()?.name;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower")]
        public ActionResult Create(NWHarvest.Web.ViewModels.ListingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var pickupLocation = db.PickupLocations.Find(vm.PickupLocationId);
                var userId = User.Identity.GetUserId();
                var grower = db.Growers.Where(g => g.UserId == userId).FirstOrDefault();

                var listingToAdd = new Listing
                {
                    Id = vm.Id,
                    Product = vm.Product,
                    QuantityAvailable = vm.QuantityAvailable,
                    QuantityClaimed = vm.QuantityClaimed,
                    UnitOfMeasure = vm.UnitOfMeasure,
                    HarvestedDate = vm.HarvestDate,
                    ExpirationDate = vm.ExpirationDate,
                    CostPerUnit = vm.CostPerUnit,
                    IsAvailable = true,
                    Comments = vm.Comments,
                    PickupLocation = pickupLocation,
                    Grower = grower
                };

                db.Listings.Add(listingToAdd);
                db.SaveChanges();

                return RedirectToAction(nameof(Manage));
            }

            vm.PickupLocations = SelectListPickupLocations();
            return View(vm);
        }

        private IEnumerable<SelectListItem> SelectListPickupLocations()
        {
            var userId = User.Identity.GetUserId();
            var pickupLocations = db.PickupLocations
                .Where(p => p.Grower.UserId == userId)
                .Select(p => new SelectListItem { Value = p.id.ToString(), Text = p.name });

            return pickupLocations.ToList();
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Edit(int? id)
        {

            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = db.Listings
                .Include("PickupLocation")
                .Include("Grower")
                .Where(l => l.Grower.UserId == UserId && l.Id == id)
                .Select(l => new ListingEditViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    CostPerUnit = l.CostPerUnit,
                    IsAvailable = l.IsAvailable,
                    PickupLocationId = l.PickupLocationId,
                    Comments = l.Comments,
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    }
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            vm.PickupLocations = SelectListPickupLocations();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower")]
        public ActionResult Edit(ListingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();

                var listing = db.Listings
                    .Where(l => l.Grower.UserId == userId && l.Id == vm.Id)
                    .FirstOrDefault();

                if (listing == null)
                {
                    return HttpNotFound();
                }

                listing.Product = vm.Product;
                listing.QuantityAvailable = vm.QuantityAvailable;
                listing.UnitOfMeasure = vm.UnitOfMeasure;
                listing.HarvestedDate = vm.HarvestDate;
                listing.ExpirationDate = vm.ExpirationDate;
                listing.CostPerUnit = vm.CostPerUnit;
                listing.IsAvailable = vm.IsAvailable;
                listing.PickupLocationId = vm.PickupLocationId;
                listing.Comments = vm.Comments;

                db.SaveChanges();

                return RedirectToAction(nameof(Manage));
            }

            vm.PickupLocations = SelectListPickupLocations();
            return View(vm);
        }

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

        public ActionResult PickUp(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Listing listing = db.Listings.Find(id);
            if (listing == null)
            {
                return HttpNotFound();
            }

            return View(listing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PickUp([Bind(Include = "id")] Listing listing)
        {
            Listing saveListing = db.Listings.Find(listing.Id);
            saveListing.IsPickedUp = true;

            db.Entry(saveListing).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Claim(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Listing listing = db.Listings.Find(id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            return View(listing);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Delete(int id)
        {
            var userId = UserId;
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = db.Listings
                .Include("PickupLocation")
                .Include("Grower")
                .Where(l => l.Grower.UserId == userId && l.Id == id)
                .Select(l => new ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    CostPerUnit = l.CostPerUnit,
                    IsAvailable = l.IsAvailable,
                    PickupLocationId = l.PickupLocationId,
                    Comments = l.Comments,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Id = l.PickupLocation.id,
                        Name = l.PickupLocation.name,
                        Address = new AddressViewModel
                        {
                            Address1 = l.PickupLocation.address1,
                            Address2 = l.PickupLocation.address2,
                            Address3 = l.PickupLocation.address3,
                            Address4 = l.PickupLocation.address4,
                            City = l.PickupLocation.city,
                            State = l.PickupLocation.state,
                            Zip = l.PickupLocation.zip
                        }
                    },
                    Grower = new GrowerViewModel
                    {
                        Id = l.Grower.Id,
                        Name = l.Grower.name
                    }
                }).FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        private string UserId => User.Identity.GetUserId();

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Listing listing = db.Listings.Find(id);
            db.Listings.Remove(listing);
            db.SaveChanges();
            return RedirectToAction(nameof(Manage));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
