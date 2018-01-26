namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveListerUserIdAsKeyToListing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropColumn("dbo.FoodBankClaim", "ListingId");
            RenameColumn(table: "dbo.FoodBankClaim", name: "Listing_Id", newName: "ListingId");
            DropPrimaryKey("dbo.Listing");
            AlterColumn("dbo.FoodBankClaim", "ListingId", c => c.Int(nullable: false));
            AlterColumn("dbo.Listing", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Listing", "Id");
            CreateIndex("dbo.FoodBankClaim", "ListingId");
            AddForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing", "Id", cascadeDelete: true);
            DropColumn("dbo.FoodBankClaim", "Listing_ListerUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FoodBankClaim", "Listing_ListerUserId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing");
            DropIndex("dbo.FoodBankClaim", new[] { "ListingId" });
            DropPrimaryKey("dbo.Listing");
            AlterColumn("dbo.Listing", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.FoodBankClaim", "ListingId", c => c.Int());
            AddPrimaryKey("dbo.Listing", new[] { "Id", "ListerUserId" });
            RenameColumn(table: "dbo.FoodBankClaim", name: "ListingId", newName: "Listing_Id");
            AddColumn("dbo.FoodBankClaim", "ListingId", c => c.Int(nullable: false));
            CreateIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            AddForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
        }
    }
}
