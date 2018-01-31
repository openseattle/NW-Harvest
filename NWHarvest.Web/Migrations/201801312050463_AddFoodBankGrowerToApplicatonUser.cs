namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFoodBankGrowerToApplicatonUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FoodBank_Id", c => c.Int());
            AddColumn("dbo.AspNetUsers", "Grower_Id", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "FoodBank_Id");
            CreateIndex("dbo.AspNetUsers", "Grower_Id");
            AddForeignKey("dbo.AspNetUsers", "FoodBank_Id", "dbo.FoodBank", "Id");
            AddForeignKey("dbo.AspNetUsers", "Grower_Id", "dbo.Grower", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Grower_Id", "dbo.Grower");
            DropForeignKey("dbo.AspNetUsers", "FoodBank_Id", "dbo.FoodBank");
            DropIndex("dbo.AspNetUsers", new[] { "Grower_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "FoodBank_Id" });
            DropColumn("dbo.AspNetUsers", "Grower_Id");
            DropColumn("dbo.AspNetUsers", "FoodBank_Id");
        }
    }
}
