using System.Web.Mvc;
using NWHarvest.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace NWHarvest.Web.Controllers
{
    

    public class HomeController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var messageService = new DisplayMessageService(_db);

            var text = messageService.GetMessages(DisplayMessageService.HomePage);
            
            return View(text);
        }

        public ActionResult RegistrationComplete()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}