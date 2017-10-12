using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NWHarvest.Web.Models;
using Microsoft.AspNet.Identity;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class GrowersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(db.Growers.ToList());
        }

        // todo: use UserRole enum
        [Authorize(Roles = "Grower")]
        public ActionResult RoleDetails()
        {
            if (UserId == null)
            {
                return HttpNotFound();
            }

            var grower = db.Growers
                .Where(fb => fb.UserId == UserId)
                .FirstOrDefault();

            if (grower == null)
            {
                return HttpNotFound();
            }

            return View(grower);
        }

        public ActionResult RoleEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grower grower = db.Growers.Find(id);
            if (grower == null)
            {
                return HttpNotFound();
            }
            return View(grower);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleEdit([Bind(Include = "id,UserId,NotificationPreference,name,phone,email,address1,address2,address3,address4,city,state,zip,IsActive")] Grower grower)
        {
            if (ModelState.IsValid)
            {
                db.Entry(grower).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction(nameof(RoleDetails), new { UserId = grower.UserId });
            }
            return View(grower);
        }


        // GET: Growers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grower grower = db.Growers.Find(id);
            if (grower == null)
            {
                return HttpNotFound();
            }
            return View(grower);
        }

        // GET: Growers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Growers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name,phone,email,address1,address2,address3,address4,city,state,zip")] Grower grower)
        {
            if (ModelState.IsValid)
            {
                db.Growers.Add(grower);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(grower);
        }

        // GET: Growers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grower grower = db.Growers.Find(id);
            if (grower == null)
            {
                return HttpNotFound();
            }
            return View(grower);
        }

        // POST: Growers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,UserId,NotificationPreference,name,phone,email,address1,address2,address3,address4,city,state,zip,IsActive")] Grower grower)
        {
            if (ModelState.IsValid)
            {
                db.Entry(grower).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(grower);
        }

        // GET: Growers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grower grower = db.Growers.Find(id);
            if (grower == null)
            {
                return HttpNotFound();
            }
            return View(grower);
        }

        // POST: Growers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Grower grower = db.Growers.Find(id);
            var aspNetUser = db.Users.Find(grower.UserId);
            db.Growers.Remove(grower);
            db.Users.Remove(aspNetUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Register(Grower grower)
        {
            db.Growers.Add(grower);
            db.SaveChanges();
            return RedirectToAction("ConfirmEmail", "Account", new { Registration = true });
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
