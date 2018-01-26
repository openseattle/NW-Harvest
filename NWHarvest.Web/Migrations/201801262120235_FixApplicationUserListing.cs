namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixApplicationUserListing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropIndex("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropPrimaryKey("dbo.Listing");
            AlterColumn("dbo.FoodBankClaim", "Listing_ListerUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Listing", "ListerUserId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Listing", new[] { "Id", "ListerUserId" });
            CreateIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            CreateIndex("dbo.Listing", "ListerUserId");
            AddForeignKey("dbo.Listing", "ListerUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
            DropColumn("dbo.AspNetUsers", "Listing_Id");
            DropColumn("dbo.AspNetUsers", "Listing_ListerUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Listing_ListerUserId", c => c.String(maxLength: 50));
            AddColumn("dbo.AspNetUsers", "Listing_Id", c => c.Int());
            DropForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropForeignKey("dbo.Listing", "ListerUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Listing", new[] { "ListerUserId" });
            DropIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropPrimaryKey("dbo.Listing");
            AlterColumn("dbo.Listing", "ListerUserId", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.FoodBankClaim", "Listing_ListerUserId", c => c.String(maxLength: 50));
            AddPrimaryKey("dbo.Listing", new[] { "Id", "ListerUserId" });
            CreateIndex("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" });
            CreateIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            AddForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
            AddForeignKey("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
        }
    }
}
