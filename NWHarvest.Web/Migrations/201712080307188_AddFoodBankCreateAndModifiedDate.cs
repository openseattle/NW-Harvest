namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFoodBankCreateAndModifiedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FoodBank", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.FoodBank", "ModifiedOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FoodBank", "ModifiedOn");
            DropColumn("dbo.FoodBank", "CreatedOn");
        }
    }
}
