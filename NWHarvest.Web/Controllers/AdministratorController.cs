using NWHarvest.Web.Models;
using System.Web.Mvc;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            return View();
        }
    }
}