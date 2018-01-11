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
        public virtual DbSet<FoodBankClaim> FoodBankClaims { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.Address1).HasColumnName("Address1"));
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.Address2).HasColumnName("Address2"));
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.City).HasColumnName("City"));
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.State).HasColumnName("State"));
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.County).HasColumnName("County"));
            modelBuilder.Types<FoodBankClaim>()
                .Configure(fbc => fbc.Property(a => a.Address.Zip).HasColumnName("Zip"));
        }
    }
}