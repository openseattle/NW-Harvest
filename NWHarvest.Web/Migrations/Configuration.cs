using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NWHarvest.Web.Models;
using System.Data.Entity.Migrations;
using System.Linq;
using NWHarvest.Web.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Text;

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
            CreateGrowerListings(context);
        }

        private void CreateGrowerListings(ApplicationDbContext context)
        {
            // abort if no growers
            if (!context.Growers.Any())
            {
                return;
            }

            var numberOfGrowers = context.Growers.Count();
            // create listing for random growers
            Random random = new Random();
            for (int i = 1; i < 400; i++)
            {
                var growerId = random.Next(1, numberOfGrowers);
                var grower = context.Growers.Find(growerId);
                var pickupLocationId = context.PickupLocations.Where(p => p.Grower.Id == growerId).First().id;
                if (grower != null)
                {
                    var listing = new Listing
                    {
                        Id = i,
                        Product = "Product1",
                        QuantityAvailable = 10,
                        QuantityClaimed = 0,
                        UnitOfMeasure = "lbs",
                        HarvestedDate = DateTime.UtcNow,
                        ExpirationDate = DateTime.UtcNow.AddDays(30),
                        CostPerUnit = 0,
                        IsAvailable = true,
                        IsPickedUp = false,
                        Grower = grower,
                        PickupLocationId = pickupLocationId
                    };
                    
                    context.Listings.Add(listing);
                }
            }

            SaveChanges(context);
        }

        private void CreateUsers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            CreateGrowers(context, userManager);
            CreateFoodBanks(context, userManager);
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

            SaveChanges(context);
        }

        private void CreateFoodBanks(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var foodBankName = "FoodBank";
            var emailDomain = "example.com";
            var foodBankPassword = "Pass@word!";
            for (int i = 1; i < 25; i++)
            {
                var user = new ApplicationUser
                {
                    Email = $"{foodBankName}{i}@{emailDomain}",
                    UserName = $"{foodBankName}{i}@{emailDomain}",
                    PasswordHash = new PasswordHasher().HashPassword(foodBankPassword),
                    EmailConfirmed = true
                };
                userManager.Create(user);

                var foodBankToAdd = new FoodBank
                {
                    Id = i,
                    UserId = user.Id,
                    name = $"{foodBankName} {i}",
                    email = $"{foodBankName}{i}@{emailDomain}",
                    address1 = $"{i} Broad St",
                    city = "Seattle",
                    state = "WA",
                    zip = "98102",
                    NotificationPreference = UserNotification.Email.ToString(),
                    IsActive = true
                };

                context.FoodBanks.AddOrUpdate<FoodBank>(foodBankToAdd);
            }

            SaveChanges(context);
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

        /// <summary>
        /// Wrapper for SaveChanges adding the Validation Messages to the generated exception
        /// </summary>
        /// <param name="context">The context.</param>
        private void SaveChanges(ApplicationDbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                // Add the original exception as the innerException
                throw new DbEntityValidationException("Entity Validation Failed - errors follow:\n" + sb.ToString(), ex); 
            }
        }
    }
}
