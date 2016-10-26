using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NWHarvest.Web.Models;

namespace NWHarvest.Web.Controllers
{
    using System;
    using System.Collections.Generic;

    public class ViewLists
    {
        public RegisteredUser registeredUser { get; set; } 
        public IEnumerable<Listing> TopList { get; set; }
        public IEnumerable<Listing> BottomList { get; set; }
        public IEnumerable<PickupLocation> PickupLocations { get; set; }
    }

    [Authorize]
    public class ListingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private const int DAY_LIMIT_FOR_GROWERS = 31;
        private const int DAY_LIMIT_FOR_FOOD_BANKS = 31;
        private const int DAY_LIMIT_FOR_ADMINISTRATORS = 180;

        // GET: Listings
        public ActionResult Index()
        {
            var registeredUserService = new RegisteredUserService();
            var user = registeredUserService.GetRegisteredUser(this.User);

            var repo = new ListingsRepository();
            var viewLists = new ViewLists();
            viewLists.registeredUser = user;
            
            if (user.Role == UserRoles.AdministratorRole)
            {
                viewLists.TopList = repo.GetAllAvailable();
                viewLists.BottomList = repo.GetAllUnavailableExpired(DAY_LIMIT_FOR_ADMINISTRATORS);
            }

            else if (user.Role == UserRoles.GrowerRole)
            {
                viewLists.TopList = repo.GetAvailableByGrower(user.GrowerId);
                viewLists.BottomList = repo.GetUnavailableExpired(user.GrowerId, DAY_LIMIT_FOR_GROWERS);
            }

            else if (user.Role == UserRoles.FoodBankRole)
            {
                viewLists.TopList = repo.GetAllAvailable();
                viewLists.BottomList = repo.GetClaimedByFoodBank(user.FoodBankId, DAY_LIMIT_FOR_FOOD_BANKS);
            }
            
            return View(viewLists);
        }

        // GET: Listings/Details/5
        public ActionResult Details(int? id)
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

        // GET: Listings/Create
        public ActionResult Create()
        {
            var registeredUserService = new RegisteredUserService();
            var user = registeredUserService.GetRegisteredUser(this.User);

            var grower = db.Growers.Where(g => g.Id == user.GrowerId).FirstOrDefault();
            ListingViewModel listingViewModel = new ListingViewModel(db, grower);
            
            return View(listingViewModel);
        }

        // POST: Listings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ListingViewModel listingViewModel)
        {   
            var service = new RegisteredUserService();
            var user = service.GetRegisteredUser(this.User);

            var saveListing = new Listing();

            saveListing.Grower = (from b in db.Growers
                            where b.Id == user.GrowerId
                            select b).FirstOrDefault();

            saveListing.PickupLocation = (from b in db.PickupLocations
                                  where b.id.ToString() == listingViewModel.SavedLocationId
                                  select b).FirstOrDefault();
            
            saveListing.product = listingViewModel.product;
            saveListing.qtyClaimed = listingViewModel.qtyClaimed;
            saveListing.qtyOffered = listingViewModel.qtyOffered;
            saveListing.qtyLabel = listingViewModel.qtyLabel;
            saveListing.harvested_date = listingViewModel.harvested_date;
            saveListing.expire_date = listingViewModel.expire_date;
            saveListing.cost = listingViewModel.cost;
            saveListing.available = true;
            saveListing.comments = listingViewModel.comments;
            saveListing.FoodBank = listingViewModel.FoodBank;
            saveListing.location = "";
            saveListing.qtyClaimed = 0;

            CheckListingForErrors(saveListing);

            if (ModelState.IsValid)
            {
                db.Listings.Add(saveListing);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //Get pickup locations.
            listingViewModel.Grower = saveListing.Grower;
            listingViewModel.PopulatePickupLocations(db);// PickupLocations = listingViewModel.PickupLocations;

            return View(listingViewModel);
        }

        private void CheckListingForErrors(Listing saveListing)
        {
            if (saveListing.product == null)
            {
                ModelState.AddModelError("Product", "Product is required.");
            }

            if (saveListing.qtyOffered == null)
            {
                ModelState.AddModelError("qtyOffered", "Quantity is required.");
            }

            if (saveListing.qtyLabel == null)
            {
                ModelState.AddModelError("qtyLabel", "Unit of Measure is required.");
            }

            if (saveListing.expire_date == null)
            {
                ModelState.AddModelError("expire_date", "Expiration Date is required.");
            }

            if (saveListing.cost == null)
            {
                ModelState.AddModelError("cost", "Cost is required.");
            }

            if (saveListing.available == null)
            {
                ModelState.AddModelError("available", "Available is required.");
            }

            if (saveListing.PickupLocation == null)
            {
                ModelState.AddModelError("PickupLocations", "You must select a pickup location. If none are available, create one under My Locations.");
            }
        }

        // GET: Listings/Edit/5
        public ActionResult Edit(int? id)
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

            var pickupLocationsForGrower = db.PickupLocations.Where(m => m.Grower.Id == listing.Grower.Id);
            ViewBag.locationsList = new SelectList(pickupLocationsForGrower, "id", "name", listing.PickupLocation.id);

            ListingViewModel listingViewModel = new ListingViewModel(db, listing.Grower);
            listingViewModel.id = Convert.ToInt32(id);
            listingViewModel.PopulatePickupLocations(db);
            listingViewModel.product = listing.product;
            listingViewModel.qtyOffered = listing.qtyOffered;
            listingViewModel.qtyLabel = listing.qtyLabel;
            listingViewModel.harvested_date = listing.harvested_date;
            listingViewModel.expire_date = listing.expire_date;
            listingViewModel.cost = listing.cost;
            listingViewModel.available = listing.available;
            listingViewModel.comments = listing.comments;

            return View(listingViewModel);
        }

        // POST: Listings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ListingViewModel listingViewModel)
        {
            listingViewModel.PopulatePickupLocations(db);
            listingViewModel.PopulatePickupLocation(db);

            var saveListing = (from b in db.Listings where b.id == listingViewModel.id select b).FirstOrDefault();

            var pickupLocationsForGrower = db.PickupLocations.Where(m => m.Grower.Id == saveListing.Grower.Id);
            ViewBag.locationsList = new SelectList(pickupLocationsForGrower, "id", "name", saveListing.PickupLocation.id);

            saveListing.product = listingViewModel.product;
            saveListing.qtyOffered = listingViewModel.qtyOffered;
            saveListing.qtyLabel = listingViewModel.qtyLabel;
            saveListing.harvested_date = listingViewModel.harvested_date;
            saveListing.expire_date = listingViewModel.expire_date;
            saveListing.cost = listingViewModel.cost;
            saveListing.available = listingViewModel.available;
            saveListing.comments = listingViewModel.comments;
            saveListing.PickupLocation = listingViewModel.PickupLocation;

            CheckListingForErrors(saveListing);

            if (ModelState.IsValid)
            {
                db.Entry(saveListing).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(listingViewModel);
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

        // GET: Listings/Claim/5
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

        // POST: Listings/Claim/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Claim([Bind(Include = "id,product,comments")] Listing listing)
        {
            var id = listing.id;



            //if (ModelState.IsValid)
            //{
                var theComments = listing.comments;
                listing = db.Listings.FirstOrDefault(p => p.id == listing.id);

                var service = new RegisteredUserService();
                var user = service.GetRegisteredUser(this.User);

                var foodBank = (from b in db.FoodBanks
                                where b.Id == user.FoodBankId
                                select b).FirstOrDefault();

                var growerUser = UserManager.FindById(listing.Grower.UserId);
                var grower = db.Growers.First(x => x.UserId == growerUser.Id);
                var message = new IdentityMessage
                {
                    Destination = growerUser.PhoneNumber,
                    Body = $"Your listing of {listing.product} has been claimed by {foodBank.name}",
                    Subject = $"NW Harvest listing of {listing.product} has been claimed by {foodBank.name}"
                };

                var sendSMS = !string.IsNullOrWhiteSpace(growerUser.PhoneNumber) &&
                              growerUser.PhoneNumberConfirmed &&
                              (grower.NotificationPreference.ToLower().Contains("both") ||
                               grower.NotificationPreference.ToLower().Contains("text"));

                var sendEmail = growerUser.EmailConfirmed && (grower.NotificationPreference.ToLower().Contains("both") ||
                              grower.NotificationPreference.ToLower().Contains("email"));

                if (sendSMS)
                {
                    UserManager.SmsService.SendAsync(message).Wait();
                }
                if (sendEmail)
                {
                    UserManager.EmailService.SendAsync(message);
                }

                listing.FoodBank = foodBank;

                listing.comments = theComments;
                listing.available = false;

                db.Entry(listing).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            //}
            //ViewBag.Grower = new SelectList(db.Growers, "id", "name", listing.Grower.Id);
            //return View(listing);
        }

        // GET: Listings/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Listing listing = db.Listings.Find(id);
            db.Listings.Remove(listing);
            db.SaveChanges();
            return RedirectToAction("Index");
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
