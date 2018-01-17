namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddListingIdToFoodBankClaim : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FoodBankClaim", "ListingId", c => c.Int(nullable: false));
            CreateIndex("dbo.FoodBankClaim", "ListingId");
            AddForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing");
            DropIndex("dbo.FoodBankClaim", new[] { "ListingId" });
            DropColumn("dbo.FoodBankClaim", "ListingId");
        }
    }
}
