namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCounty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FoodBank", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
            AddColumn("dbo.Grower", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
            AddColumn("dbo.PickupLocation", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PickupLocation", "county");
            DropColumn("dbo.Grower", "county");
            DropColumn("dbo.FoodBank", "county");
        }
    }
}
