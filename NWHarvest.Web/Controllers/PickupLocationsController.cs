using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using NWHarvest.Web.Helper;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.Enums;
using System.Linq.Expressions;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class PickupLocationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string _userRoleSessionKey = "UserRole";
        private IQueryable<PickupLocation> _queryFoodBankPickupLocations => db.PickupLocations.Where(fb => fb.FoodBank.UserId == UserId);
        private IQueryable<PickupLocation> _queryGrowerPickupLocations => db.PickupLocations.Where(g => g.Grower.UserId == UserId);
        private IQueryable<FoodBank> _queryFoodBank => db.FoodBanks.Where(fb => fb.UserId == UserId);
        private IQueryable<Grower> _queryGrower => db.Growers.Where(g => g.UserId == UserId);

        public PickupLocationsController()
        {
            SetSessionUserRole();
        }
        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Manage()
        {
            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    ViewBag.Name = db.FoodBanks.Where(fb => fb.UserId == UserId).FirstOrDefault()?.name;
                    ViewBag.BackToProfile = UserRole.FoodBank.ToString() + "s";
                    return View(GetPickupLocations(_queryFoodBankPickupLocations).ToList());
                default:
                    ViewBag.Name = db.Growers.Where(g => g.UserId == UserId).FirstOrDefault()?.Name;
                    ViewBag.BackToProfile = UserRole.Grower.ToString() + "s";
                    return View(GetPickupLocations(_queryGrowerPickupLocations).ToList());
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = GetPickupLocation(id.Value);

            if (vm == null)
            {
                return HttpNotFound();
            }

            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Create()
        {
            PickupLocationEditViewModel vm = InitializePickupLocationEditViewModel();
            if (vm == null)
            {
                return HttpNotFound();
            }

            RegisterViewData();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,FoodBank")]
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
                    comments = vm.Comments
                };

                if (User.IsInRole(UserRole.FoodBank.ToString()))
                {
                    pickupLocationToAdd.FoodBank = db.FoodBanks.Where(fb => fb.UserId == UserId).First();
                }
                else
                {
                    pickupLocationToAdd.Grower = db.Growers.Where(g => g.UserId == UserId).First();
                }

                db.PickupLocations.Add(pickupLocationToAdd);
                db.SaveChanges();

                return RedirectToAction(nameof(Manage));
            }

            RegisterViewData();
            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return HttpNotFound();
            }

            var location = GetPickupLocation(id.Value);

            var vm = new PickupLocationEditViewModel
            {
                Id = location.Id,
                Name = location.Name,
                UserName = location.UserName,
                Comments = location.Comments,
                Address = new AddressEditViewModel
                {
                    Address1 = location.Address.Address1,
                    Address2 = location.Address.Address2,
                    Address3 = location.Address.Address3,
                    Address4 = location.Address.Address4,
                    City = location.Address.City,
                    County = location.Address.County,
                    State = location.Address.State,
                    Zip = location.Address.Zip
                }
            };

            if (vm == null)
            {
                return HttpNotFound();
            }

            RegisterViewData();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Edit(PickupLocationEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                PickupLocation pickupLocationToUpdate = db.PickupLocations.Find(vm.Id);
                if (pickupLocationToUpdate == null)
                {
                    return HttpNotFound();
                }
                pickupLocationToUpdate.name = vm.Name;
                pickupLocationToUpdate.address1 = vm.Address.Address1;
                pickupLocationToUpdate.address2 = vm.Address.Address2;
                pickupLocationToUpdate.address3 = vm.Address.Address3;
                pickupLocationToUpdate.address4 = vm.Address.Address4;
                pickupLocationToUpdate.city = vm.Address.City;
                pickupLocationToUpdate.state = vm.Address.State;
                pickupLocationToUpdate.county = vm.Address.County;
                pickupLocationToUpdate.zip = vm.Address.Zip;
                pickupLocationToUpdate.comments = vm.Comments;

                db.SaveChanges();
                return RedirectToAction(nameof(Manage));
            }

            RegisterViewData();
            return View(vm);
        }

        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult Delete(int? id)
        {
            if (id == null || UserId == null)
            {
                return HttpNotFound();
            }

            var vm = GetPickupLocation(id.Value);

            if (vm == null)
            {
                return HttpNotFound();
            }
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower,FoodBank")]
        public ActionResult DeleteConfirmed(int id)
        {
            var pickupLocationToRemove = db.PickupLocations.Find(id);
            if (pickupLocationToRemove == null)
            {
                return HttpNotFound();
            }

            if (db.Listings.Where(l => l.PickupLocationId == id).Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete location. One or more listings depends on this location.");
                return View(GetPickupLocation(id));
            }

            try
            {
                db.PickupLocations.Remove(pickupLocationToRemove);
                db.SaveChanges();
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "You cannot delete this location because it is used on an existing Listing.");
                return View(GetPickupLocation(id));
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

        #region Helpers
        private void SetSessionUserRole()
        {
            if (System.Web.HttpContext.Current.User.IsInRole(UserRole.FoodBank.ToString()))
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.FoodBank;
            else
                System.Web.HttpContext.Current.Session[_userRoleSessionKey] = UserRole.Grower;
        }
        private PickupLocationViewModel GetPickupLocation(int id)
        {
            PickupLocationViewModel vm;
            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    vm = GetPickupLocations(_queryFoodBankPickupLocations, id).First();
                    vm.UserName = _queryFoodBank.First().name;
                    return vm;
                default:
                    vm = GetPickupLocations(_queryGrowerPickupLocations, id).First();
                    vm.UserName = _queryGrower.First().Name;
                    return vm;
            }
        }
        private IEnumerable<PickupLocationViewModel> GetPickupLocations(IQueryable<PickupLocation> query, int id = 0)
        {
            if (id != 0)
            {
                query = query.Where(q => q.id == id);
            }

            return query.Select(p => new PickupLocationViewModel
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
                    County = p.county,
                    Zip = p.zip
                }
            });
        }
        private PickupLocationEditViewModel InitializePickupLocationEditViewModel()
        {
            switch (Session[_userRoleSessionKey])
            {
                case UserRole.FoodBank:
                    return _queryFoodBank.Select(fb => new PickupLocationEditViewModel
                    {
                        UserName = fb.name,
                        Address = new AddressEditViewModel
                        {
                            County = fb.county,
                            State = fb.state
                        },
                    }).FirstOrDefault();
                default:
                    return _queryGrower.Select(g => new PickupLocationEditViewModel
                    {
                        UserName = g.Name,
                        Address = new AddressEditViewModel
                        {
                            County = g.County,
                            State = g.State
                        },
                    }).FirstOrDefault();
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
