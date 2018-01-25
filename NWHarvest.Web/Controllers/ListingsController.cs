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
using NWHarvest.Web.Enums;
using NWHarvest.Web.Helper;
using System.Linq.Expressions;

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
        private NotificationManager notificationManager = new NotificationManager();
        private readonly string _userRoleSessionKey = "UserRole";
        private IQueryable<FoodBank> _queryFoodBank => db.FoodBanks.Where(fb => fb.UserId == UserId);
        private IQueryable<Grower> _queryGrower => db.Growers.Where(g => g.UserId == UserId);

        private readonly IQueryable<PickupLocation> _queryPickupLocations;
        private readonly IQueryable<Listing> _queryListings;
        private const int DAY_LIMIT_FOR_GROWERS = 31;
        private const int DAY_LIMIT_FOR_FOOD_BANKS = 31;
        private const int DAY_LIMIT_FOR_ADMINISTRATORS = 180;

        public ListingsController()
        {
            if (System.Web.HttpContext.Current.User.IsInRole(UserRole.FoodBank.ToString()))
            {
                _queryPickupLocations = db.PickupLocations.Where(fb => fb.FoodBank.UserId == UserId);
                _queryListings = db.Listings.Where(fb => fb.FoodBank.UserId == UserId);
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.FoodBank;
            }
            else
            {
                _queryPickupLocations = db.PickupLocations.Where(fb => fb.Grower.UserId == UserId);
                _queryListings = db.Listings.Where(fb => fb.Grower.UserId == UserId);
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.Grower;
            }
        }

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

        [Authorize(Roles = "FoodBank")]
        public ActionResult Search(string searchString, ListingStatus status = ListingStatus.Available)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["ListingStatus"] = status.ToString();

            var query = db.Listings.Where(l => l.IsAvailable == true);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Product.Contains(searchString));
            }

            var vm = query.ToList().Select(l => new ListingViewModel
            {
                Id = l.Id,
                Product = l.Product,
                QuantityAvailable = l.QuantityAvailable,
                UnitOfMeasure = l.UnitOfMeasure,
                CostPerUnit = l.CostPerUnit,
                ExpirationDate = l.ExpirationDate,
                Comments = l.Comments
            });

            return View(vm);
        }

        
        public ActionResult Manage()
        {
            var vm = _queryListings
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
                    Comments = l.Comments
                }).ToList();
            
            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Details(int id)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = _queryListings
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

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Create()
        {
            var vm = new ListingViewModel
            {
                PickupLocations = SelectListPickupLocations()
            };
            vm.UserName = GetUserName();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Create(ListingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var pickupLocation = db.PickupLocations.Find(vm.PickupLocationId);
                var userId = User.Identity.GetUserId();
                
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
                    PickupLocation = pickupLocation
                };

                switch (Session[_userRoleSessionKey])
                {
                    case UserRole.FoodBank:
                        listingToAdd.FoodBank = _queryFoodBank.FirstOrDefault();
                        break;
                    case UserRole.Grower:
                        listingToAdd.Grower = _queryGrower.FirstOrDefault();
                        break;
                    default:
                        return View("Error");
                }

                db.Listings.Add(listingToAdd);
                db.SaveChanges();

                //var foodBanksToNotify = db.FoodBanks.Where(f => f.county != "Unknown" && f.county == pickupLocation.county).ToList();
                //if (foodBanksToNotify.Count > 0)
                //{
                //    SendNotification(listingToAdd, foodBanksToNotify);
                //}

                return RedirectToAction(nameof(Manage));
            }

            vm.PickupLocations = SelectListPickupLocations();
            vm.UserName = GetUserName();
            return View(vm);
        }

        private IEnumerable<SelectListItem> SelectListPickupLocations()
        {
            return _queryPickupLocations
                .Select(p => new SelectListItem { Value = p.id.ToString(), Text = p.name })
                .ToList();
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
                var newPickupLocation = db.PickupLocations.Find(vm.PickupLocationId);
                var foodBanksToNotify = db.FoodBanks.Where(f => f.county != "Unknown" && f.county == newPickupLocation.county).ToList();

                var listing = db.Listings
                    .Where(l => l.Grower.UserId == userId && l.Id == vm.Id)
                    .Include(l => l.Grower)
                    .Include(l => l.PickupLocation)
                    .FirstOrDefault();

                if (listing == null)
                {
                    return HttpNotFound();
                }

                var prevPickupLocation = listing.PickupLocation;

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

                if (prevPickupLocation.county != newPickupLocation.county && foodBanksToNotify.Count > 0)
                {
                    SendNotification(listing, foodBanksToNotify);
                }

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

        [Authorize(Roles = "Grower,Administrator")]
        public ActionResult Delete(int id, string returnUrl = null)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var query = db.Listings
                .Include("PickupLocation")
                .Include("Grower")
                .AsQueryable();

            if (User.IsInRole(UserRole.Administrator.ToString()))
            {
                ViewBag.CancelActionLink = returnUrl;
                query = query.Where(l => l.Id == id);
            } else
            {
                ViewBag.CancelActionLink = Url.Action("Profile", "Growers", null);
                query = query.Where(l => l.Grower.UserId == UserId && l.Id == id);
            }

            var vm = query
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
        public ActionResult DeleteConfirmed(int id, string returnUrl = null)
        {
            Listing listing = db.Listings.Find(id);
            db.Listings.Remove(listing);
            db.SaveChanges();

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Manage));
        }

        // send sms and/or email notification to Food Banks when a new listing is added in their county
        private void SendNotification(Listing listing, List<FoodBank> foodBanks)
        {
            var growerPhoneNumber = notificationManager.GetUserPhoneNumber(listing.Grower.UserId);

            var subject = $"NW Harvest listing of {listing.Product} has just been added by {listing.Grower.name}";
            var body = $"Listing of {listing.Product} has been added by {listing.Grower.name}, {listing.Grower.email}";

            if (growerPhoneNumber != null)
            {
                body += ", " + growerPhoneNumber;
            }

            foreach (var fb in foodBanks)
            {
                var foodBankPhoneNumber = notificationManager.GetUserPhoneNumber(fb.UserId);
                var message = new NotificationMessage
                {
                    DestinationPhoneNumber = foodBankPhoneNumber,
                    DestinationEmailAddress = fb.email,
                    Subject = subject,
                    Body = body
                };
                notificationManager.SendNotification(message, fb.NotificationPreference);
            }
        }
        private string GetUserName()
        {
            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    return _queryFoodBank.First().name;
                case UserRole.Grower:
                    return _queryGrower.First().name;
                default:
                    return string.Empty;
            }
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
