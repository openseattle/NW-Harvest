namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddListerColumnsToListing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing");
            DropIndex("dbo.FoodBankClaim", new[] { "ListingId" });
            RenameColumn(table: "dbo.Listing", name: "FoodBank_Id", newName: "FoodBankId");
            RenameColumn(table: "dbo.Listing", name: "Grower_Id", newName: "GrowerId");
            RenameIndex(table: "dbo.Listing", name: "IX_Grower_Id", newName: "IX_GrowerId");
            RenameIndex(table: "dbo.Listing", name: "IX_FoodBank_Id", newName: "IX_FoodBankId");
            DropPrimaryKey("dbo.Listing");
            AddColumn("dbo.FoodBankClaim", "Listing_Id", c => c.Int());
            AddColumn("dbo.FoodBankClaim", "Listing_ListerUserId", c => c.String(maxLength: 50));
            AddColumn("dbo.Listing", "ListerUserId", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Listing", "ListerRole", c => c.String());
            AddColumn("dbo.AspNetUsers", "Listing_Id", c => c.Int());
            AddColumn("dbo.AspNetUsers", "Listing_ListerUserId", c => c.String(maxLength: 50));
            AlterColumn("dbo.Listing", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Listing", new[] { "Id", "ListerUserId" });
            CreateIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            CreateIndex("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" });
            AddForeignKey("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
            AddForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing", new[] { "Id", "ListerUserId" });
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropForeignKey("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" }, "dbo.Listing");
            DropIndex("dbo.AspNetUsers", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropIndex("dbo.FoodBankClaim", new[] { "Listing_Id", "Listing_ListerUserId" });
            DropPrimaryKey("dbo.Listing");
            AlterColumn("dbo.Listing", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.AspNetUsers", "Listing_ListerUserId");
            DropColumn("dbo.AspNetUsers", "Listing_Id");
            DropColumn("dbo.Listing", "ListerRole");
            DropColumn("dbo.Listing", "ListerUserId");
            DropColumn("dbo.FoodBankClaim", "Listing_ListerUserId");
            DropColumn("dbo.FoodBankClaim", "Listing_Id");
            AddPrimaryKey("dbo.Listing", "Id");
            RenameIndex(table: "dbo.Listing", name: "IX_FoodBankId", newName: "IX_FoodBank_Id");
            RenameIndex(table: "dbo.Listing", name: "IX_GrowerId", newName: "IX_Grower_Id");
            RenameColumn(table: "dbo.Listing", name: "GrowerId", newName: "Grower_Id");
            RenameColumn(table: "dbo.Listing", name: "FoodBankId", newName: "FoodBank_Id");
            CreateIndex("dbo.FoodBankClaim", "ListingId");
            AddForeignKey("dbo.FoodBankClaim", "ListingId", "dbo.Listing", "Id", cascadeDelete: true);
        }
    }
}
