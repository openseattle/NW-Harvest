namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGrowerCreateModifiedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Grower", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Grower", "ModifiedOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Grower", "ModifiedOn");
            DropColumn("dbo.Grower", "CreatedOn");
        }
    }
}
