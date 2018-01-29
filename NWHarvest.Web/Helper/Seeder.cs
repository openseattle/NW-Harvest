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
            CreateGrowerListings();
            CreateFoodBankListings();
        }
        
        private void CreateGrowerListings()
        {
            // abort if no growers
            if (!_context.Growers.Any())
            {
                return;
            }
            
            var numberOfGrowers = _context.Growers.Count();
            var numberOfListings = _context.Listings.Count();
            Random random = new Random();
            for (int i = numberOfListings + 1; i < numberOfListings + 4*numberOfGrowers; i++)
            {
                var harvestDate = RandomDateTime();
                var growerId = random.Next(1, numberOfGrowers);
                var grower = _context.Growers.Find(growerId);
                var pickupLocationId = _context.PickupLocations.Where(p => p.Grower.Id == growerId).First().id;
                if (grower != null)
                {
                    AddListing(i, pickupLocationId, grower.UserId, UserRole.Grower.ToString());
                }
            }
            SaveChanges();
        }

        private void CreateFoodBankListings()
        {
            // abort if no foodbanks
            if (!_context.FoodBanks.Any())
            {
                return;
            }

            var numberOfFoodbanks = _context.FoodBanks.Count();
            var numberOfListings = _context.Listings.Count();
            Random random = new Random();
            for (int i = numberOfListings + 1; i < numberOfListings + 4*numberOfFoodbanks; i++)
            {
                var foodbankId = random.Next(1, numberOfFoodbanks);
                var foodbankUser = _context.FoodBanks.Include("PickupLocations").Where(f => f.Id == foodbankId).Select(f => new
                {
                    UserId = f.UserId,
                    PickuplocationId = f.PickupLocations.FirstOrDefault().id
                }).FirstOrDefault();
                
                AddListing(i, foodbankUser.PickuplocationId, foodbankUser.UserId, UserRole.FoodBank.ToString());
            }
            SaveChanges();
        }

        private void AddListing(int id, int pickupLocationId, string listerUserId, string role)
        {
            Random random = new Random();
            var harvestDate = RandomDateTime();
            var listing = new Listing
            {
                Id = id,
                Product = RandomProduct(),
                QuantityAvailable = random.Next(1, 100),
                QuantityClaimed = 0,
                UnitOfMeasure = "lb",
                HarvestedDate = harvestDate,
                ExpirationDate = harvestDate.AddDays(30),
                CostPerUnit = Math.Round((decimal)random.Next(1,100)/100),
                IsAvailable = true,
                IsPickedUp = false,
                PickupLocationId = pickupLocationId,
                ListerRole = role,
                ListerUserId = listerUserId
            };
            _context.Listings.Add(listing);
        }

        private void CreateUsers()
        {
            CreateAdministrators();
            CreateGrowers();
            CreateFoodBanks();
        }

        private void CreateFoodBanks()
        {
            var random = new Random();
            var foodBankName = "FoodBank";
            var emailDomain = "example.com";
            var foodBankPassword = "Pass@word!";
            var cities = WashingtonCities();
            var counties = WashingtonCounties();
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

                var createdOn = DateTime.Today.AddDays(random.Next(-180, 0));
                var zip = random.Next(98001, 99403);
                var city = cities[random.Next(0, cities.Count() - 1)];
                var county = counties[random.Next(0, counties.Count() - 1)];

                var foodBankToAdd = new FoodBank
                {
                    Id = i,
                    UserId = user.Id,
                    name = $"{foodBankName} {i}",
                    email = $"{foodBankName.ToLower()}{i}@{emailDomain}",
                    address1 = $"{i} Broad St",
                    city = city,
                    state = "WA",
                    county = county,
                    zip = zip.ToString(),
                    NotificationPreference = UserNotification.Email.ToString(),
                    IsActive = true,
                    CreatedOn = createdOn,
                    ModifiedOn = createdOn,
                    PickupLocations = new List<PickupLocation>
                    {
                        new PickupLocation
                        {
                            name = "Default",
                            address1 = $"{i} Foodbank St",
                            city = city,
                            state = "WA",
                            county = county,
                            zip = zip.ToString()
                        }
                    }
                };

                _context.FoodBanks.AddOrUpdate<FoodBank>(foodBankToAdd);
            }

            SaveChanges();
        }

        private void CreateGrowers()
        {
            var random = new Random();
            var cities = WashingtonCities();
            var counties = WashingtonCounties();
            var growerName = "Grower";
            var emailDomain = "example.com";
            var growerPassword = "Pass@word!";
            for (int i = 1; i < 100; i++)
            {
                var user = new ApplicationUser
                {
                    Email = $"{growerName.ToLower()}{i}@{emailDomain}",
                    UserName = $"{growerName}{i}@{emailDomain}",
                    PasswordHash = new PasswordHasher().HashPassword(growerPassword),
                    EmailConfirmed = true
                };
                _userManager.Create(user);
                _userManager.AddToRole(user.Id, UserRole.Grower.ToString());

                var createdOn = DateTime.Today.AddDays(random.Next(-180, 0));
                var zip = random.Next(98001, 99403);
                var city = cities[random.Next(0, cities.Count() - 1)];
                var county = counties[random.Next(0, counties.Count() - 1)];
                var growerToAdd = new Grower
                {
                    Id = i,
                    UserId = user.Id,
                    name = $"{growerName} {i}",
                    email = $"{growerName.ToLower()}{i}@{emailDomain}",
                    address1 = $"{i} Main St",
                    city = city,
                    state = "WA",
                    county = county,
                    zip = zip.ToString(),
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
                            city = city,
                            state = "WA",
                            county = county,
                            zip = zip.ToString()
                        }
                    }
                };
                _context.Growers.AddOrUpdate<Grower>(growerToAdd);
            }

            SaveChanges();
        }

        private void CreateAdministrators()
        {
            string username = ConfigurationManager.AppSettings["adminUsername"] ?? "admin@northwestharvest.org";
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
                    var quantityClaimed = random.Next(1, (int)listing.QuantityAvailable);
                    var foodbankClaim = new FoodBankClaim
                    {
                        Listing = listing,
                        FoodBankId = random.Next(1, numberOfFoodBanks),
                        Product = listing.Product,
                        Quantity = quantityClaimed,
                        CostPerUnit = listing.CostPerUnit,
                        Address = new Address
                        {
                            Address1 = listing.PickupLocation.address1,
                            Address2 = listing.PickupLocation.address2,
                            City = listing.PickupLocation.city,
                            State = listing.PickupLocation.state,
                            County = listing.PickupLocation.county,
                            Zip = listing.PickupLocation.zip
                        }
                    };
                    listing.QuantityAvailable -= quantityClaimed;
                    listing.QuantityClaimed += quantityClaimed;
                    _context.FoodBankClaims.Add(foodbankClaim);
                }
                isOdd = !isOdd;
            }
            _context.SaveChanges();
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
            var produces = new string[]
            {
                "Kale",
                "Apples",
                "Summer Squash",
                "Blackberries",
                "Beets",
                "Melon",
                "Carrots",
                "Chard",
                "Broccoli",
                "Mushrooms",
                "Potatoes",
                "Winter Squash",
                "Tomatoes",
                "Peppers",
                "Onion",
                "Cucumbers",
                "Spinach",
                "Herbs",
                "Lettuce",
                "Eggplant"
            };
            Random random = new Random();
            return produces[random.Next(0, produces.Count()-1)];
        }

        private string[] WashingtonCities()
        {
            return new string[]
            {
                "Aberdeen",
                "Airway Heights",
                "Algona",
                "Anacortes",
                "Arlington",
                "Asotin",
                "Auburn",
                "Bainbridge Island",
                "Battle Ground",
                "Bellevue",
                "Bellingham",
                "Benton City",
                "Bingen",
                "Black Diamond",
                "Blaine",
                "Bonney Lake",
                "Bothell",
                "Bremerton",
                "Brewster",
                "Bridgeport",
                "Brier",
                "Buckley",
                "Burien",
                "Burlington",
                "Camas",
                "Carnation",
                "Cashmere",
                "Castle Rock",
                "Centralia",
                "Chehalis",
                "Chelan",
                "Cheney",
                "Chewelah",
                "Clarkston",
                "Cle Elum",
                "Clyde Hill",
                "Colfax",
                "College Place",
                "Colville",
                "Connell",
                "Cosmopolis",
                "Covington",
                "Davenport",
                "Dayton",
                "Deer Park",
                "Des Moines",
                "DuPont",
                "Duvall",
                "East Wenatchee",
                "Edgewood",
                "Edmonds",
                "Electric City",
                "Ellensburg",
                "Elma",
                "Entiat",
                "Enumclaw",
                "Ephrata",
                "Everett",
                "Everson",
                "Federal Way",
                "Ferndale",
                "Fife",
                "Fircrest",
                "Forks",
                "George",
                "Gig Harbor",
                "Gold Bar",
                "Goldendale",
                "Grand Coulee",
                "Grandview",
                "Granger",
                "Granite Falls",
                "Harrington",
                "Hoquiam",
                "Ilwaco",
                "Issaquah",
                "Kahlotus",
                "Kalama",
                "Kelso",
                "Kenmore",
                "Kennewick",
                "Kent",
                "Kettle Falls",
                "Kirkland",
                "Kittitas",
                "La Center",
                "Lacey",
                "Lake Forest Park",
                "Lake Stevens",
                "Lakewood",
                "Langley",
                "Leavenworth",
                "Liberty Lake",
                "Long Beach",
                "Longview",
                "Lynden",
                "Lynnwood",
                "Mabton",
                "Maple Valley",
                "Marysville",
                "Mattawa",
                "McCleary",
                "Medical Lake",
                "Medina",
                "Mercer Island",
                "Mesa",
                "Mill Creek",
                "Millwood",
                "Milton",
                "Monroe",
                "Montesano",
                "Morton",
                "Moses Lake",
                "Mossyrock",
                "Mount Vernon",
                "Mountlake Terrace",
                "Moxee",
                "Mukilteo",
                "Napavine",
                "Newcastle",
                "Newport",
                "Nooksack",
                "Normandy Park",
                "North Bend",
                "North Bonneville",
                "Oak Harbor",
                "Oakville",
                "Ocean Shores",
                "Okanogan",
                "Olympia",
                "Omak",
                "Oroville",
                "Orting",
                "Othello",
                "Pacific",
                "Palouse",
                "Pasco",
                "Pateros",
                "Pomeroy",
                "Port Angeles",
                "Port Orchard",
                "Port Townsend",
                "Poulsbo",
                "Prescott",
                "Prosser",
                "Pullman",
                "Puyallup",
                "Quincy",
                "Rainier",
                "Raymond",
                "Redmond",
                "Renton",
                "Republic",
                "Richland",
                "Ridgefield",
                "Ritzville",
                "Rock Island",
                "Roslyn",
                "Roy",
                "Royal City",
                "Ruston",
                "Sammamish",
                "SeaTac",
                "Seattle",
                "Sedro-Woolley",
                "Selah",
                "Sequim",
                "Shelton",
                "Shoreline",
                "Snohomish",
                "Snoqualmie",
                "Soap Lake",
                "South Bend",
                "Spangle",
                "Spokane",
                "Spokane Valley",
                "Sprague",
                "Stanwood",
                "Stevenson",
                "Sultan",
                "Sumas",
                "Sumner",
                "Sunnyside",
                "Tacoma",
                "Tekoa",
                "Tenino",
                "Tieton",
                "Toledo",
                "Tonasket",
                "Toppenish",
                "Tukwila",
                "Tumwater",
                "Union Gap",
                "University Place",
                "Vader",
                "Vancouver",
                "Waitsburg",
                "Walla Walla",
                "Wapato",
                "Warden",
                "Washougal",
                "Wenatchee",
                "West Richland",
                "Westport",
                "White Salmon",
                "Winlock",
                "Woodinville",
                "Woodland",
                "Woodway",
                "Yakima",
                "Yelm",
                "Zillah"
            };
        }
        private string[] WashingtonCounties()
        {
            return WashingtonState.GetCounties().ToArray();
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