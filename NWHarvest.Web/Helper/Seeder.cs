using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NWHarvest.Web.Enums;
using NWHarvest.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace NWHarvest.Web.Helper
{
    public class Seeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserStore<ApplicationUser> _userStore;
        private readonly UserManager<ApplicationUser> _userManager;

        public Seeder(ApplicationDbContext context)
        {
            _context = context;
            _userStore = new UserStore<ApplicationUser>(context);
            _userManager = new UserManager<ApplicationUser>(_userStore);
        }

        public void Populate()
        {
            CreateRoles();
            CreateUsers();
            CreateListings();
            FoodBankRandomClaims();
        }

        private void CreateListings()
        {
            // abort if no growers
            if (!_context.Growers.Any())
            {
                return;
            }

            var numberOfGrowers = _context.Growers.Count();
            // create listing for random growers
            Random random = new Random();
            for (int i = 1; i < 400; i++)
            {
                var harvestDate = RandomDateTime();
                var growerId = random.Next(1, numberOfGrowers);
                var grower = _context.Growers.Find(growerId);
                var pickupLocationId = _context.PickupLocations.Where(p => p.Grower.Id == growerId).First().id;
                if (grower != null)
                {
                    var listing = new Listing
                    {
                        Id = i,
                        Product = RandomProduct(),
                        QuantityAvailable = 10,
                        QuantityClaimed = 0,
                        UnitOfMeasure = "lbs",
                        HarvestedDate = harvestDate,
                        ExpirationDate = harvestDate.AddDays(30),
                        CostPerUnit = 0,
                        IsAvailable = true,
                        IsPickedUp = false,
                        Grower = grower,
                        PickupLocationId = pickupLocationId
                    };

                    _context.Listings.Add(listing);
                }
            }

            SaveChanges();
        }

        private void CreateUsers()
        {
            CreateAdministrators();
            CreateGrowers();
            CreateFoodBanks();
        }

        private void CreateFoodBanks()
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
                _userManager.Create(user);
                _userManager.AddToRole(user.Id, UserRole.FoodBank.ToString());

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

                _context.FoodBanks.AddOrUpdate<FoodBank>(foodBankToAdd);
            }

            SaveChanges();
        }

        private void CreateGrowers()
        {
            var random = new Random();
            var growerName = "Grower";
            var emailDomain = "example.com";
            var growerPassword = "Pass@word!";
            for (int i = 1; i < 100; i++)
            {
                var user = new ApplicationUser
                {
                    Email = $"{growerName}{i}@{emailDomain}",
                    UserName = $"{growerName}{i}@{emailDomain}",
                    PasswordHash = new PasswordHasher().HashPassword(growerPassword),
                    EmailConfirmed = true
                };
                _userManager.Create(user);
                _userManager.AddToRole(user.Id, UserRole.Grower.ToString());

                var createdOn = DateTime.Today.AddDays(random.Next(-180, 0));
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
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn,
                    PickupLocations = new List<PickupLocation>
                    {
                        new PickupLocation
                        {
                            name = "Default",
                            address1 = $"{i} Main St",
                            city = "Seattle",
                            state = "WA",
                            zip = "98102"
                        }
                    }
                };
                _context.Growers.AddOrUpdate<Grower>(growerToAdd);
            }

            SaveChanges();
        }

        private void CreateAdministrators()
        {
            string username = ConfigurationManager.AppSettings["adminUsername"] ?? "admin@northwestharvest.com";
            string password = ConfigurationManager.AppSettings["adminPassword"] ?? "Pass@word1";
            var user = new ApplicationUser
            {
                UserName = username,
                Email = username,
                PasswordHash = new PasswordHasher().HashPassword(password),
                EmailConfirmed = true
            };
            _userManager.Create(user);
            _userManager.AddToRole(user.Id, UserRole.Administrator.ToString());
            SaveChanges();
        }

        private void CreateRoles()
        {
            var roleStore = new RoleStore<IdentityRole>(_context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            // abort if roles already exist
            if (roleStore.Roles.Any())
            {
                return;
            }

            var roles = Enum.GetValues(typeof(UserRole));
            foreach (var item in roles)
            {
                var role = new IdentityRole(item.ToString());
                roleManager.Create(role);
            }
        }

        // set half of expired listings as claimed by a random food bank
        private void FoodBankRandomClaims()
        {
            // abort if there is no listings or foodbanks
            if (!_context.Listings.Any() || !_context.FoodBanks.Any())
            {
                return;
            }

            var today = DateTime.UtcNow;
            var expiredListings = _context.Listings
                .Where(l => l.ExpirationDate < today)
                .ToList();

            bool isOdd = true;
            var numberOfFoodBanks = _context.FoodBanks.Count();
            var random = new Random();
            foreach (var listing in expiredListings)
            {
                if (isOdd)
                {
                    var foodBank = _context.FoodBanks.Find(random.Next(1, numberOfFoodBanks));
                    listing.IsAvailable = false;
                    listing.IsPickedUp = true;
                    listing.FoodBank = foodBank;
                    _context.SaveChanges();
                }
                isOdd = !isOdd;
            }
        }

        // randomly create dates within six months (past/future) of the current date
        private DateTime RandomDateTime()
        {
            Random random = new Random();
            DateTime start = DateTime.UtcNow.AddDays(-180);
            DateTime end = DateTime.Today.AddDays(180);
            int range = (end - start).Days;

            return start.AddDays(random.Next(range));
        }

        private string RandomProduct()
        {
            var maxNumberOfProducts = 10;
            string[] products = new string[maxNumberOfProducts];
            for (int i = 0; i < maxNumberOfProducts; i++)
            {
                products[i] = $"Product {i + 1}";
            }

            Random random = new Random();

            return products[random.Next(0, maxNumberOfProducts)];
        }

        /// <summary>
        /// Wrapper for SaveChanges adding the Validation Messages to the generated exception
        /// </summary>
        /// <param name="context">The context.</param>
        private void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
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