using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace NWHarvest.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public virtual DbSet<Grower> Growers { get; set; }
        public virtual DbSet<FoodBank> FoodBanks { get; set; }
        public virtual DbSet<Listing> Listings { get; set; }
        public virtual DbSet<PickupLocation> PickupLocations { get; set; }
        public virtual DbSet<DisplayMessage> DisplayMessages { get; set; }
        public virtual DbSet<DisplayDescription> DisplayDescriptions { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<County> Counties { get; set; }
    }
}