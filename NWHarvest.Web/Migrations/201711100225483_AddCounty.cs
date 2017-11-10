namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCounty : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.County",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        StateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.State", t => t.StateId, cascadeDelete: true)
                .Index(t => t.StateId);
            
            CreateTable(
                "dbo.State",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShortName = c.String(nullable: false, maxLength: 2),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FoodBank", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
            AddColumn("dbo.Grower", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
            AddColumn("dbo.PickupLocation", "county", c => c.String(nullable: false, maxLength: 50, defaultValue: "Unknown"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.County", "StateId", "dbo.State");
            DropIndex("dbo.County", new[] { "StateId" });
            DropColumn("dbo.PickupLocation", "county");
            DropColumn("dbo.Grower", "county");
            DropColumn("dbo.FoodBank", "county");
            DropTable("dbo.State");
            DropTable("dbo.County");
        }
    }
}
