namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFoodBankClaimTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FoodBankClaim",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Product = c.String(),
                        Quantity = c.Int(nullable: false),
                        CostPerUnit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Address1 = c.String(maxLength: 200),
                        Address2 = c.String(maxLength: 200),
                        City = c.String(maxLength: 100),
                        State = c.String(maxLength: 2),
                        County = c.String(nullable: false, maxLength: 50),
                        Zip = c.String(maxLength: 9),
                        GrowerId = c.Int(nullable: false),
                        FoodBankId = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FoodBank", t => t.FoodBankId, cascadeDelete: true)
                .ForeignKey("dbo.Grower", t => t.GrowerId, cascadeDelete: true)
                .Index(t => t.GrowerId)
                .Index(t => t.FoodBankId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FoodBankClaim", "GrowerId", "dbo.Grower");
            DropForeignKey("dbo.FoodBankClaim", "FoodBankId", "dbo.FoodBank");
            DropIndex("dbo.FoodBankClaim", new[] { "FoodBankId" });
            DropIndex("dbo.FoodBankClaim", new[] { "GrowerId" });
            DropTable("dbo.FoodBankClaim");
        }
    }
}
