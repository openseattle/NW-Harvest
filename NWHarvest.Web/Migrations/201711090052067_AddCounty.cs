namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCounty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FoodBank", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Not Applicable"));
            AddColumn("dbo.Grower", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Not Applicable"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Grower", "county");
            DropColumn("dbo.FoodBank", "county");
        }
    }
}
