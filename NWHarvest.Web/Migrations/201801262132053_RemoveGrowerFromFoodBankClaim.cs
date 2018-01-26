namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveGrowerFromFoodBankClaim : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FoodBankClaim", "GrowerId", "dbo.Grower");
            DropIndex("dbo.FoodBankClaim", new[] { "GrowerId" });
            DropColumn("dbo.FoodBankClaim", "GrowerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FoodBankClaim", "GrowerId", c => c.Int(nullable: false));
            CreateIndex("dbo.FoodBankClaim", "GrowerId");
            AddForeignKey("dbo.FoodBankClaim", "GrowerId", "dbo.Grower", "Id", cascadeDelete: true);
        }
    }
}
