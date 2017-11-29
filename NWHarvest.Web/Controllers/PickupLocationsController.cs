using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using Microsoft.AspNet.Identity;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class PickupLocationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Grower")]
        public ActionResult Manage()
        {
            var vm = db.PickupLocations
                .Where(pl => pl.Grower.UserId == UserId)
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

            ViewBag.GrowerName = db.Growers.Where(g => g.UserId == UserId).FirstOrDefault()?.name;

            return View(vm);
        }

        public ActionResult Details(int? id)
        {
            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .Select(p => new PickupLocationViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Grower = new GrowerViewModel
                    {
                        Name = p.Grower.name
                    },
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
                }).FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Create()
        {
            var vm = db.Growers
                .Where(g => g.UserId == UserId)
                .Select(g => new PickupLocationEditViewModel
                {
                    Grower = new GrowerViewModel
                    {
                        Id = g.Id,
                        Name = g.name
                    },
                    Address = new AddressEditViewModel
                    {
                        County = g.county,
                        State = g.state
                    }
                })
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
        public ActionResult Create(PickupLocationEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var pickupLocationToAdd = new PickupLocation
                {
                    name = vm.Name,
                    address1 = vm.Address.Address1,
                    address2 = vm.Address.Address2,
                    address3 = vm.Address.Address3,
                    address4 = vm.Address.Address4,
                    city = vm.Address.City,
                    county = vm.Address.County,
                    state = vm.Address.State,
                    zip = vm.Address.Zip,
                    comments = vm.Comments,
                    Grower = db.Growers.Find(vm.Grower.Id)
                };

                db.PickupLocations.Add(pickupLocationToAdd);
                db.SaveChanges();

                return RedirectToAction(nameof(Manage));
            }

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var vm = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .Select(p => new PickupLocationEditViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Address = new AddressEditViewModel
                    {
                        Address1 = p.address1,
                        Address2 = p.address2,
                        Address3 = p.address3,
                        Address4 = p.address4,
                        City = p.city,
                        County = p.county,
                        State = p.state,
                        Zip = p.zip
                    },
                    Grower = new GrowerViewModel
                    {
                        Name = p.Grower.name
                    }
                })
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
        public ActionResult Edit(PickupLocationEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var pickupLocationToUpdate = db.PickupLocations
                    .Where(p => p.id == vm.Id && p.Grower.UserId == UserId)
                    .FirstOrDefault();

                pickupLocationToUpdate.name = vm.Name;
                pickupLocationToUpdate.address1 = vm.Address.Address1;
                pickupLocationToUpdate.address2 = vm.Address.Address2;
                pickupLocationToUpdate.address3 = vm.Address.Address3;
                pickupLocationToUpdate.address4 = vm.Address.Address4;
                pickupLocationToUpdate.city = vm.Address.City;
                pickupLocationToUpdate.zip = vm.Address.Zip;
                pickupLocationToUpdate.comments = vm.Comments;

                db.SaveChanges();
                return RedirectToAction(nameof(Manage));
            }

            return View(vm);
        }

        [Authorize(Roles = "Grower")]
        public ActionResult Delete(int? id)
        {
            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .Select(p => new PickupLocationViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Grower = new GrowerViewModel
                    {
                        Name = p.Grower.name
                    },
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
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower")]
        public ActionResult DeleteConfirmed(int id)
        {
            // check if location is associated with any listings
            if (db.Listings.Where(l => l.PickupLocationId == id && UserId == l.Grower.UserId).ToList().Count > 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete location. One or more listings depends on this location.");
                var vm = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .Select(p => new PickupLocationViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Grower = new GrowerViewModel
                    {
                        Name = p.Grower.name
                    },
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
                .FirstOrDefault();
                return View(vm);
            }

            PickupLocation pickupLocation = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .FirstOrDefault();

            try
            {
                db.PickupLocations.Remove(pickupLocation);
                db.SaveChanges();
                return RedirectToAction(nameof(Manage));
            }

            catch (Exception)
            {
                var vm = db.PickupLocations
                .Where(p => p.id == id && p.Grower.UserId == UserId)
                .Select(p => new PickupLocationViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Comments = p.comments,
                    Grower = new GrowerViewModel
                    {
                        Name = p.Grower.name
                    },
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
                .FirstOrDefault();

                ModelState.AddModelError(String.Empty, "You cannot delete this location because it is used on an existing Listing.");
                return View(vm);
            }
        }

        private string UserId => User.Identity.GetUserId();

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
