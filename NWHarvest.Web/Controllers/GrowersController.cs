using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.ViewModels;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class GrowersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // todo: use UserRole enum
        [Authorize(Roles = "Grower")]
        [ActionName("Profile")]
        public ActionResult Index()
        {

            var vm = db.Growers
                .Where(g => g.UserId == UserId)
                .Include("PickupLocations")
                .Include("Listings")
                .Select(g => new GrowerViewModel
                {
                    Id = g.Id,
                    Name = g.name,
                    Email = g.email,
                    Address = new AddressViewModel
                    {
                        Address1 = g.address1,
                        Address2 = g.address2,
                        Address3 = g.address3,
                        Address4 = g.address4,
                        City = g.city,
                        State = g.state,
                        Zip = g.zip
                    },
                    IsActive = g.IsActive,
                    NotificationPreference = g.NotificationPreference
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            vm.PickupLocations = db.PickupLocations
                .Where(p => p.Grower.UserId == UserId)
                .Select(p => new PickupLocationViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Address = new AddressViewModel
                    {
                        Address1 = p.address1,
                        Address2 = p.address2,
                        Address3 = p.address3,
                        Address4 = p.address4,
                        City = p.city,
                        State = p.state,
                        Zip = p.zip
                    }
                })
                .ToList();

            vm.Listings = db.Listings
                .Where(l => l.Grower.UserId == UserId)
                .Select(l => new ViewModels.ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    ExpirationDate = l.ExpirationDate,
                    Comments = l.Comments,
                    IsAvailable = l.IsAvailable,
                    QuantityClaimed = l.QuantityClaimed,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Name = l.PickupLocation.name
                    }

                }).ToList();

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = db.Growers.Select(g => new RoleEditViewModel
            {
                Id = g.Id,
                Name = g.name,
                Address1 = g.address1,
                Address2 = g.address2,
                Address3 = g.address3,
                Address4 = g.address4,
                City = g.city,
                State = g.state,
                Zip = g.zip,
                IsActive = g.IsActive,
                NotificationPreference = g.NotificationPreference
            })
            .Where(g => g.Id == id)
            .FirstOrDefault();


            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower")]
        public ActionResult Edit(RoleEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var grower = db.Growers.Find(vm.Id);
                grower.name = vm.Name;
                grower.address1 = vm.Address1;
                grower.address2 = vm.Address2;
                grower.address3 = vm.Address3;
                grower.address4 = vm.Address4;
                grower.city = vm.City;
                grower.state = vm.State;
                grower.zip = vm.Zip;
                grower.IsActive = vm.IsActive;
                db.SaveChanges();

                return RedirectToAction(nameof(Profile), new { UserId = grower.UserId });
            }
            return View(vm);
        }

        [AllowAnonymous]
        public ActionResult Register(Grower grower)
        {
            // create default pickup location for new user
            var pickupLocation = new PickupLocation
            {
                name = "Default",
                address1 = grower.address1,
                address2 = grower.address2,
                address3 = grower.address3,
                address4 = grower.address4,
                city = grower.city,
                state = grower.state,
                zip = grower.zip
            };

            grower.PickupLocations.Add(pickupLocation);
            db.Growers.Add(grower);
            db.SaveChanges();
            return RedirectToAction("ConfirmEmail", "Account", new { Registration = true });
        }

        public ActionResult Settings()
        {
            return RedirectToAction("Index", "Manage");
        }

        public string UserId => User.Identity.GetUserId();

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
