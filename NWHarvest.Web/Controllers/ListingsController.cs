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
using System;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class ListingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        private NotificationManager notificationManager = new NotificationManager();
        private readonly string _userRoleSessionKey = "UserRole";
        private IQueryable<FoodBank> _queryFoodBank => db.FoodBanks.Where(fb => fb.UserId == UserId);
        private IQueryable<Grower> _queryGrower => db.Growers.Where(g => g.UserId == UserId);
        private readonly IQueryable<PickupLocation> _queryPickupLocations;
        private IQueryable<Listing> _queryListings => db.Listings.Where(l => l.ListerUserId == UserId);
        private string UserId => User.Identity.GetUserId();

        public ListingsController()
        {
            if (System.Web.HttpContext.Current.User.IsInRole(UserRole.FoodBank.ToString()))
            {
                _queryPickupLocations = db.PickupLocations.Where(fb => fb.FoodBank.UserId == UserId);
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.FoodBank;
            }
            else
            {
                _queryPickupLocations = db.PickupLocations.Where(fb => fb.Grower.UserId == UserId);
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.Grower;
            }
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

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Index()
        {
            var vm = _queryListings
                .Where(l => l.ListerUserId == UserId)
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

            ViewBag.UserName = GetUserName();
            ViewBag.ProfileUrl = GetProfileUrl();
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
                .Where(l => l.Id == id)
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
            vm.UserName = GetUserName();
            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Create()
        {
            var vm = new ListingViewModel
            {
                HarvestDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
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
                var listingToAdd = new Listing
                {
                    Id = vm.Id,
                    ListerUserId = UserId,
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
                        listingToAdd.ListerRole = UserRole.FoodBank.ToString();
                        break;
                    case UserRole.Grower:
                        listingToAdd.ListerRole = UserRole.Grower.ToString();
                        break;
                    default:
                        return View("Error");
                }

                db.Listings.Add(listingToAdd);
                db.SaveChanges();

                var foodBanksToNotify = db.FoodBanks.Where(f => f.county != "Unknown" && f.county == pickupLocation.county).ToList();
                if (foodBanksToNotify.Count > 0)
                {
                    SendNotification(listingToAdd, foodBanksToNotify);
                }

                return RedirectToAction(nameof(Index));
            }

            vm.PickupLocations = SelectListPickupLocations();
            vm.UserName = GetUserName();
            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Edit(int? id)
        {

            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = _queryListings
                .Where(l => l.Id == id.Value)
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
                    Comments = l.Comments
                }).FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            vm.PickupLocations = SelectListPickupLocations();
            vm.UserName = GetUserName();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Edit(ListingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var listing = _queryListings
                    .Include("PickupLocation")
                    .Where(l => l.Id == vm.Id)
                    .FirstOrDefault();

                if (listing == null)
                {
                    return HttpNotFound();
                }

                var prevPickupLocation = db.PickupLocations.Where(p => p.id == listing.PickupLocation.id).First();
                var newPickupLocation = db.PickupLocations.Find(vm.PickupLocationId);

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

                var foodBanksToNotify = db.FoodBanks.Where(f => f.county != "Unknown" && f.county == newPickupLocation.county).ToList();
                if (prevPickupLocation.county != newPickupLocation.county && foodBanksToNotify.Count > 0)
                {
                    SendNotification(listing, foodBanksToNotify);
                }

                return RedirectToAction(nameof(Index));
            }

            vm.PickupLocations = SelectListPickupLocations();
            return View(vm);
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

        [Authorize(Roles = "Grower,Administrator,FoodBank")]
        public ActionResult Delete(int id, string returnUrl = null)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    ViewBag.CancelActionLink = Url.Action("Profile", "FoodBanks", null);
                    break;
                case UserRole.Grower:
                    ViewBag.CancelActionLink = Url.Action("Profile", "Growers", null);
                    break;
                case UserRole.Administrator:
                    ViewBag.CancelActionLink = returnUrl;
                    break;
                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = _queryListings
                .Where(l => l.Id == id)
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
                    }
                }).FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }
            vm.UserName = GetUserName();
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,Administrator,FoodBank")]
        public ActionResult DeleteConfirmed(int id, string returnUrl = null)
        {
            Listing listing = db.Listings.Find(id);
            db.Listings.Remove(listing);
            db.SaveChanges();

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // send sms and/or email notification to Food Banks when a new listing is added in their county
        private void SendNotification(Listing listing, List<FoodBank> foodBanks)
        {
            var lister = db.Listings
                        .Include("User")
                        .Where(l => l.Id == listing.Id)
                        .Select(l => new ListerViewModel
                        {
                            Email = l.User.Email,
                            PhoneNumber = l.User.PhoneNumber,
                            Role = l.ListerRole
                        }).First();
            
            switch ((ListerRole)Enum.Parse(typeof(ListerRole), lister.Role))
            {
                case ListerRole.Grower:
                    lister.Name = db.Growers.Where(g => g.UserId == UserId).First().name;
                    break;
                case ListerRole.FoodBank:
                    lister.Name = db.FoodBanks.Where(g => g.UserId == UserId).First().name;
                    break;
                default:
                    return;
            }
            var subject = $"NW Harvest listing";
            var body = $"Listing of {listing.Product} has been added by {lister.Name}, {lister.Email}";

            if (lister.PhoneNumber != null)
            {
                body += ", " + lister.PhoneNumber;
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
                case UserRole.Administrator:
                    return UserRole.Administrator.ToString();
                default:
                    return string.Empty;
            }
        }

        private string GetProfileUrl()
        {
            string profile = "Profile";
            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    return Url.Action(profile, "FoodBanks");
                case UserRole.Grower:
                    return Url.Action(profile, "Growers");
                case UserRole.Administrator:
                    return Url.Action("Index", "Administrator");
                default:
                    return Url.Action("Index");
            }
        }

        private IEnumerable<SelectListItem> SelectListPickupLocations()
        {
            return _queryPickupLocations
                .Select(p => new SelectListItem { Value = p.id.ToString(), Text = p.name })
                .ToList();
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
