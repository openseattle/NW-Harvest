using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class FoodBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Register(FoodBank foodbank)
        {
            db.FoodBanks.Add(foodbank);
            db.SaveChanges();

            return RedirectToAction("ConfirmEmail", "Account", new { Registration = true });
        }

        public ActionResult Index()
        {
            return View(db.FoodBanks.ToList());
        }

        // todo: use UserRole enum
        [Authorize(Roles = "FoodBank")]
        public ActionResult RoleDetails()
        {
            if (UserId == null)
            {
                return HttpNotFound();
            }
            var foodBank = db.FoodBanks
                .Where(fb => fb.UserId == UserId)
                .FirstOrDefault();
            
            if (foodBank == null)
            {
                return HttpNotFound();
            }

            return View(foodBank);
        }

        public ActionResult Details(int? id)
        {
            FoodBank foodBank = db.FoodBanks.Find(id);
            if (foodBank == null)
            {
                return HttpNotFound();
            }
            return View(foodBank);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,phone,email,address1,address2,address3,address4,city,state,zip")] FoodBank foodBank)
        {
            if (ModelState.IsValid)
            {
                db.FoodBanks.Add(foodBank);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(foodBank);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FoodBank foodBank = db.FoodBanks.Find(id);
            if (foodBank == null)
            {
                return HttpNotFound();
            }
            return View(foodBank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,NotificationPreference,id,name,phone,email,address1,address2,address3,address4,city,state,zip,IsActive")] FoodBank foodBank)
        {
            if (ModelState.IsValid)
            {
                db.Entry(foodBank).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(foodBank);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FoodBank foodBank = db.FoodBanks.Find(id);
            if (foodBank == null)
            {
                return HttpNotFound();
            }
            return View(foodBank);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FoodBank foodBank = db.FoodBanks.Find(id);
            var aspNetUser = db.Users.Find(foodBank.UserId);
            db.FoodBanks.Remove(foodBank);
            db.Users.Remove(aspNetUser);
            db.SaveChanges();
            return RedirectToAction("Index");
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
