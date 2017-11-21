using NWHarvest.Web.Enums;
using NWHarvest.Web.Models;
using NWHarvest.Web.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace NWHarvest.Web.Controllers
{
    [Authorize]
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private const int DAY_LIMIT_FOR_ADMINISTRATORS = 180;


        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var vm = new AdministratorViewModel();
            vm.NumberOfGrowers = db.Growers.Count();

            return View(vm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageUser(UserRole userRole)
        {
            switch (userRole)
            {
                case UserRole.Administrator:
                    break;
                case UserRole.Grower:
                    return RedirectToAction(nameof(ManageGrowers));
                case UserRole.FoodBank:
                    break;
                default:
                    break;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageGrowers()
        {
            var vm = db.Growers
                .OrderBy(g => g.name)
                .Select(g => new GrowerViewModel
                {
                    Id = g.Id,
                    Name = g.name,
                    IsActive = g.IsActive
                }).ToList();

            return View(vm);
        }
    }
}