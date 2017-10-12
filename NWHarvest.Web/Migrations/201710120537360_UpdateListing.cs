namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateListing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Listing", "PickupLocation_id", "dbo.PickupLocation");
            DropIndex("dbo.Listing", new[] { "PickupLocation_id" });
            RenameColumn(table: "dbo.Listing", name: "PickupLocation_id", newName: "PickupLocationId");
            AddColumn("dbo.Listing", "QuantityAvailable", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Listing", "QuantityClaimed", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Listing", "UnitOfMeasure", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Listing", "HarvestedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Listing", "ExpirationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Listing", "CostPerUnit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Listing", "IsAvailable", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Listing", "PickupLocationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Listing", "PickupLocationId");
            AddForeignKey("dbo.Listing", "PickupLocationId", "dbo.PickupLocation", "id", cascadeDelete: true);
            DropColumn("dbo.Listing", "qtyOffered");
            DropColumn("dbo.Listing", "qtyClaimed");
            DropColumn("dbo.Listing", "qtyLabel");
            DropColumn("dbo.Listing", "harvested_date");
            DropColumn("dbo.Listing", "expire_date");
            DropColumn("dbo.Listing", "cost");
            DropColumn("dbo.Listing", "available");
            DropColumn("dbo.Listing", "location");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Listing", "location", c => c.String());
            AddColumn("dbo.Listing", "available", c => c.Boolean());
            AddColumn("dbo.Listing", "cost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Listing", "expire_date", c => c.DateTime(nullable: false, storeType: "date"));
            AddColumn("dbo.Listing", "harvested_date", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.Listing", "qtyLabel", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Listing", "qtyClaimed", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Listing", "qtyOffered", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.Listing", "PickupLocationId", "dbo.PickupLocation");
            DropIndex("dbo.Listing", new[] { "PickupLocationId" });
            AlterColumn("dbo.Listing", "PickupLocationId", c => c.Int());
            DropColumn("dbo.Listing", "IsAvailable");
            DropColumn("dbo.Listing", "CostPerUnit");
            DropColumn("dbo.Listing", "ExpirationDate");
            DropColumn("dbo.Listing", "HarvestedDate");
            DropColumn("dbo.Listing", "UnitOfMeasure");
            DropColumn("dbo.Listing", "QuantityClaimed");
            DropColumn("dbo.Listing", "QuantityAvailable");
            RenameColumn(table: "dbo.Listing", name: "PickupLocationId", newName: "PickupLocation_id");
            CreateIndex("dbo.Listing", "PickupLocation_id");
            AddForeignKey("dbo.Listing", "PickupLocation_id", "dbo.PickupLocation", "id");
        }
    }
}
