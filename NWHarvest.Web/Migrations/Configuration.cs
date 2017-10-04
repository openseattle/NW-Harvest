using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NWHarvest.Web.Models;
using System.Data.Entity.Migrations;
using System.Linq;

namespace NWHarvest.Web.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        private string adminUsername => ConfigurationManager.AppSettings["adminUsername"] ?? "admin@northwestharvest.com";
        private string adminPassword => ConfigurationManager.AppSettings["adminPassword"] ?? "Pass@word1";

        protected override void Seed(ApplicationDbContext context)
        {
            if (context.Users.Any(u => u.UserName == adminUsername))
            {
                return;
            }
            var adminuser = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminUsername,
                PasswordHash = new PasswordHasher().HashPassword(adminPassword),
                EmailConfirmed = true
            };

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            var adminRole = new IdentityRole("Administrator");

            roleManager.Create(adminRole);
            userManager.Create(adminuser);
            userManager.AddToRole(adminuser.Id, "Administrator");
        }
    }
}
