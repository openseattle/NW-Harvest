namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFoodBankIdToPickupLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickupLocation", "FoodBank_Id", c => c.Int());
            CreateIndex("dbo.PickupLocation", "FoodBank_Id");
            AddForeignKey("dbo.PickupLocation", "FoodBank_Id", "dbo.FoodBank", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickupLocation", "FoodBank_Id", "dbo.FoodBank");
            DropIndex("dbo.PickupLocation", new[] { "FoodBank_Id" });
            DropColumn("dbo.PickupLocation", "FoodBank_Id");
        }
    }
}
