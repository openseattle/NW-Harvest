using System.Web.Mvc;
using NWHarvest.Web.Models;

namespace NWHarvest.Web.Controllers
{
    

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RegistrationComplete()
        {
            return View();
        }
    }
}