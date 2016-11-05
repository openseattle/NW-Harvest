using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NWHarvest.Web.Models;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class FoodBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FoodBanks
        public ActionResult Index()
        {
            return View(db.FoodBanks.ToList());
        }

        // GET: FoodBanks/Details/5
        public ActionResult Details(int? id)
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

        // GET: FoodBanks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FoodBanks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: FoodBanks/Edit/5
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

        // POST: FoodBanks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: FoodBanks/Delete/5
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

        // POST: FoodBanks/Delete/5
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
