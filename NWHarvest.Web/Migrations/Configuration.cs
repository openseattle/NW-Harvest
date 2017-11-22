using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NWHarvest.Web.Models;
using System.Data.Entity.Migrations;
using System.Linq;
using NWHarvest.Web.Enums;
using System;
using System.Collections.Generic;

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

            CreateRoles(roleManager);

            userManager.Create(adminuser);
            userManager.AddToRole(adminuser.Id, UserRole.Administrator.ToString());

            CreateUsers(context, userManager);
        }

        private void CreateUsers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            CreateGrowers(context, userManager);
        }

        private void CreateGrowers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var growerName = "Grower";
            var emailDomain = "example.com";
            var growerPassword = "Pass@word!";
            for (int i = 1; i < 100; i++)
            {
                var user = new ApplicationUser {
                    Email = $"{growerName}{i}@{emailDomain}",
                    UserName = $"{growerName}{i}@{emailDomain}",
                    PasswordHash = new PasswordHasher().HashPassword(growerPassword),
                    EmailConfirmed = true
                };
                userManager.Create(user);

                var growerToAdd = new Grower
                {
                    Id = i,
                    UserId = user.Id,
                    name = $"{growerName} {i}",
                    email = $"{growerName}{i}@{emailDomain}",
                    address1 = $"{i} Main St",
                    city = "Seattle",
                    state = "WA",
                    zip = "98102",
                    NotificationPreference = UserNotification.Email.ToString(),
                    IsActive = true,
                    PickupLocations = new List<PickupLocation>
                    {
                        new PickupLocation
                        {
                            name = "Default",
                            address1 = "{i} Main St",
                            city = "Seattle",
                            state = "WA",
                            zip = "98102"
                        }
                    }
                };
                
                context.Growers.AddOrUpdate<Grower>(growerToAdd);
            }

            context.SaveChanges();
        }

        private void CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = Enum.GetValues(typeof(UserRole));
            foreach (var item in roles)
            {
                var role = new IdentityRole(item.ToString());
                roleManager.Create(role);
            }
        }
    }
}
