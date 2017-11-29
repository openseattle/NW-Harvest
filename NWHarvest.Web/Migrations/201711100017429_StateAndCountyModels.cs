namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StateAndCountyModels : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.County", "StateId", "dbo.State");
            DropIndex("dbo.County", new[] { "StateId" });
            DropTable("dbo.State");
            DropTable("dbo.County");
        }
    }
}
