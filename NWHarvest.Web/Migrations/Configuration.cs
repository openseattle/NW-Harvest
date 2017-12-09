using NWHarvest.Web.Models;
using System.Data.Entity.Migrations;
using System.Linq;
using NWHarvest.Web.Helper;

namespace NWHarvest.Web.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var seeder = new Seeder(context);
            seeder.Populate();
        }
    }
}