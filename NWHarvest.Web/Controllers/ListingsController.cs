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

        // GET: Listings
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
            
            saveListing.Product = listingViewModel.product;
            saveListing.QuantityClaimed = listingViewModel.qtyClaimed.Value;
            saveListing.QuantityAvailable = listingViewModel.qtyOffered.Value;
            saveListing.UnitOfMeasure = listingViewModel.qtyLabel;
            saveListing.HarvestedDate = listingViewModel.harvested_date.Value;
            saveListing.ExpirationDate = listingViewModel.expire_date.Value;
            saveListing.CostPerUnit = listingViewModel.cost.Value;
            saveListing.IsAvailable = true;
            saveListing.Comments = listingViewModel.comments;
            saveListing.FoodBank = listingViewModel.FoodBank;
            saveListing.QuantityClaimed = 0;

            CheckListingForErrors(saveListing);

            if (ModelState.IsValid)
            {
                db.Listings.Add(saveListing);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //Get pickup locations.
            listingViewModel.Grower = saveListing.Grower;
            listingViewModel.PopulatePickupLocations(db);

            return View(listingViewModel);
        }

        private void CheckListingForErrors(Listing saveListing)
        {
            if (saveListing.Product == null)
            {
                ModelState.AddModelError("Product", "Product is required.");
            }

            if (saveListing.QuantityAvailable == null)
            {
                ModelState.AddModelError("qtyOffered", "Quantity is required.");
            }

            if (saveListing.UnitOfMeasure == null)
            {
                ModelState.AddModelError("qtyLabel", "Unit of Measure is required.");
            }

            if (saveListing.ExpirationDate == null)
            {
                ModelState.AddModelError("expire_date", "Expiration Date is required.");
            }

            if (saveListing.ExpirationDate < DateTime.Now)
            {
                ModelState.AddModelError("expire_date", "Expiration Date must be greater than or equal to today's date.");
            }

            if (saveListing.CostPerUnit == null)
            {
                ModelState.AddModelError("cost", "Cost is required.");
            }

            if (saveListing.IsAvailable == null)
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
            listingViewModel.product = listing.Product;
            listingViewModel.qtyOffered = listing.QuantityAvailable;
            listingViewModel.qtyLabel = listing.UnitOfMeasure;
            listingViewModel.harvested_date = listing.HarvestedDate;
            listingViewModel.expire_date = listing.ExpirationDate;
            listingViewModel.cost = listing.CostPerUnit;
            listingViewModel.available = listing.IsAvailable;
            listingViewModel.comments = listing.Comments;

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

            var saveListing = (from b in db.Listings where b.Id == listingViewModel.id select b).FirstOrDefault();

            var pickupLocationsForGrower = db.PickupLocations.Where(m => m.Grower.Id == saveListing.Grower.Id);
            ViewBag.locationsList = new SelectList(pickupLocationsForGrower, "id", "name", saveListing.PickupLocation.id);

            saveListing.Product = listingViewModel.product;
            saveListing.QuantityAvailable = listingViewModel.qtyOffered.Value;
            saveListing.UnitOfMeasure = listingViewModel.qtyLabel;
            saveListing.HarvestedDate = listingViewModel.harvested_date.Value;
            saveListing.ExpirationDate = listingViewModel.expire_date.Value;
            saveListing.CostPerUnit = listingViewModel.cost.Value;
            saveListing.IsAvailable = listingViewModel.available.Value;
            saveListing.Comments = listingViewModel.comments;
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

        // POST: Listings/PickUp/5
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

        // POST: Listings/PickUp/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            var id = listing.Id;

            var theComments = listing.Comments;
            listing = db.Listings.FirstOrDefault(p => p.Id == listing.Id);

            var service = new RegisteredUserService();
            var user = service.GetRegisteredUser(this.User);

            var foodBank = (from b in db.FoodBanks
                            where b.Id == user.FoodBankId
                            select b).FirstOrDefault();
            var foodBankUser = UserManager.FindById(foodBank.UserId);

            var growerUser = UserManager.FindById(listing.Grower.UserId);
            var grower = db.Growers.First(x => x.UserId == growerUser.Id);

            var sendSMS = !string.IsNullOrWhiteSpace(growerUser.PhoneNumber) &&
                            growerUser.PhoneNumberConfirmed &&
                            (grower.NotificationPreference.ToLower().Contains("both")
                            || grower.NotificationPreference.ToLower().Contains("text"));

            if (sendSMS)
            {
                var textMessage = CreateSMSMessage(growerUser, foodBankUser, listing);

                UserManager.SmsService.SendAsync(textMessage).Wait();
            }

            var sendEmail = growerUser.EmailConfirmed
                && (grower.NotificationPreference.ToLower().Contains("both")
                || grower.NotificationPreference.ToLower().Contains("email"));

            if (sendEmail)
            {
                var emailMessage = CreateEmailMessage(growerUser, foodBankUser, listing);

                UserManager.EmailService.SendAsync(emailMessage);
            }

            listing.FoodBank = foodBank;

            listing.Comments = theComments;
            listing.IsAvailable = false;

            db.Entry(listing).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private IdentityMessage CreateSMSMessage(ApplicationUser grower, ApplicationUser foodBank, Listing listing)
        {
            var body = $"Your listing of {listing.Product} has been claimed by {foodBank.UserName}, {foodBank.Email}";

            if(foodBank.PhoneNumber != null && foodBank.PhoneNumber != "")
            {
                body += $", {foodBank.PhoneNumber}";
            }

            var smsMessage = new IdentityMessage
            {
                Destination = grower.PhoneNumber,
                Subject = "",
                Body = body
            };

            return smsMessage;
        }

        private IdentityMessage CreateEmailMessage(ApplicationUser grower, ApplicationUser foodBank, Listing listing)
        {
            var subject = $"NW Harvest listing of {listing.Product} has been claimed by {foodBank.UserName}";
            var body = $"Your listing of {listing.Product} has been claimed by {foodBank.UserName}, {foodBank.Email}";

            if (foodBank.PhoneNumber != null && foodBank.PhoneNumber != "")
            {
                body += $", {foodBank.PhoneNumber}";
            }

            var emailMessage = new IdentityMessage
            {
                Destination = grower.Email,
                Subject = subject,
                Body = body
            };

            return emailMessage;
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
