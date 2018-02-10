﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NWHarvest.Web.Helper;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;
using NWHarvest.Web.ViewModels;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class GrowersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
                    Name = g.Name,
                    Email = g.Email,
                    Address = new AddressViewModel
                    {
                        Address1 = g.Address1,
                        Address2 = g.Address2,
                        Address3 = g.Address3,
                        Address4 = g.Address4,
                        City = g.City,
                        County = g.County,
                        State = g.State,
                        Zip = g.Zip
                    },
                    IsActive = g.IsActive,
                    NotificationPreference = g.NotificationPreference
                })
                .FirstOrDefault();

            if (vm == null)
            {
                return HttpNotFound();
            }

            if (!vm.IsActive)
            {
                return View("DisabledUser");
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
                        County = p.county,
                        State = p.state,
                        Zip = p.zip
                    }
                })
                .ToList();

            vm.Listings = db.Listings
                .Where(l => l.ListerUserId == UserId)
                .Select(l => new ViewModels.ListingViewModel
                {
                    Id = l.Id,
                    Product = l.Product,
                    QuantityAvailable = l.QuantityAvailable,
                    CostPerUnit = l.CostPerUnit,
                    UnitOfMeasure = l.UnitOfMeasure,
                    HarvestDate = l.HarvestedDate,
                    ExpirationDate = l.ExpirationDate,
                    Comments = l.Comments,
                    IsAvailable = l.IsAvailable,
                    IsPickedUp = l.IsPickedUp,
                    QuantityClaimed = l.QuantityClaimed,
                    PickupLocation = new PickupLocationViewModel
                    {
                        Name = l.PickupLocation.name,
                        Address = new AddressViewModel
                        {
                            City = l.PickupLocation.city,
                            County = l.PickupLocation.county,
                            Zip = l.PickupLocation.zip
                        }
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

            var vm = db.Growers.Select(g => new GrowerEditViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Address = new AddressEditViewModel
                {
                    Address1 = g.Address1,
                    Address2 = g.Address2,
                    Address3 = g.Address3,
                    Address4 = g.Address4,
                    City = g.City,
                    County = g.County,
                    State = g.State,
                    Zip = g.Zip
                },
                IsActive = g.IsActive,
                NotificationPreference = g.NotificationPreference
            })
            .Where(g => g.Id == id)
            .FirstOrDefault();


            if (vm == null)
            {
                return HttpNotFound();
            }

            RegisterViewData();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Grower")]
        public ActionResult Edit(GrowerEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var grower = db.Growers.Find(vm.Id);
                grower.Name = vm.Name;
                grower.Address1 = vm.Address.Address1;
                grower.Address2 = vm.Address.Address2;
                grower.Address3 = vm.Address.Address3;
                grower.Address4 = vm.Address.Address4;
                grower.City = vm.Address.City;
                grower.County = vm.Address.County;
                grower.State = vm.Address.State;
                grower.Zip = vm.Address.Zip;
                grower.IsActive = vm.IsActive;
                db.SaveChanges();

                return RedirectToAction(nameof(Profile), new { UserId = grower.UserId });
            }

            RegisterViewData();
            return View(vm);
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

        #region Helpers
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
