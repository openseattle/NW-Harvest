namespace NWHarvest.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DisplayDescription",
                c => new
                    {
                        DisplayDescriptionId = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.DisplayDescriptionId);
            
            CreateTable(
                "dbo.DisplayMessage",
                c => new
                    {
                        DisplayMessageId = c.Int(nullable: false, identity: true),
                        DisplayDescriptionId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Message = c.String(nullable: false, maxLength: 2000),
                    })
                .PrimaryKey(t => t.DisplayMessageId)
                .ForeignKey("dbo.DisplayDescription", t => t.DisplayDescriptionId, cascadeDelete: true)
                .Index(t => t.DisplayDescriptionId)
                .Index(t => t.SortOrder, unique: true, name: "DescriptionId_SortOrder_Unique");
            
            CreateTable(
                "dbo.FoodBank",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        name = c.String(nullable: false, maxLength: 100),
                        email = c.String(nullable: false, maxLength: 100),
                        address1 = c.String(nullable: false, maxLength: 200),
                        address2 = c.String(maxLength: 200),
                        address3 = c.String(maxLength: 200),
                        address4 = c.String(maxLength: 200),
                        city = c.String(nullable: false, maxLength: 100),
                        state = c.String(nullable: false, maxLength: 2),
                        zip = c.String(nullable: false, maxLength: 9),
                        NotificationPreference = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Grower",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        name = c.String(nullable: false, maxLength: 100),
                        email = c.String(nullable: false, maxLength: 100),
                        address1 = c.String(nullable: false, maxLength: 200),
                        address2 = c.String(maxLength: 200),
                        address3 = c.String(maxLength: 200),
                        address4 = c.String(maxLength: 200),
                        city = c.String(nullable: false, maxLength: 100),
                        state = c.String(nullable: false, maxLength: 2),
                        zip = c.String(nullable: false, maxLength: 9),
                        NotificationPreference = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PickupLocation",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(maxLength: 50),
                        address1 = c.String(maxLength: 200),
                        address2 = c.String(maxLength: 200),
                        address3 = c.String(maxLength: 200),
                        address4 = c.String(maxLength: 200),
                        city = c.String(maxLength: 100),
                        state = c.String(maxLength: 2),
                        zip = c.String(maxLength: 9),
                        comments = c.String(),
                        Grower_Id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Grower", t => t.Grower_Id)
                .Index(t => t.Grower_Id);
            
            CreateTable(
                "dbo.Listing",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        product = c.String(nullable: false),
                        qtyOffered = c.Decimal(nullable: false, precision: 18, scale: 2),
                        qtyClaimed = c.Decimal(precision: 18, scale: 2),
                        qtyLabel = c.String(nullable: false, maxLength: 100),
                        harvested_date = c.DateTime(storeType: "date"),
                        expire_date = c.DateTime(nullable: false, storeType: "date"),
                        cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        available = c.Boolean(),
                        comments = c.String(),
                        location = c.String(),
                        IsPickedUp = c.Boolean(nullable: false),
                        FoodBank_Id = c.Int(),
                        Grower_Id = c.Int(),
                        PickupLocation_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.FoodBank", t => t.FoodBank_Id)
                .ForeignKey("dbo.Grower", t => t.Grower_Id)
                .ForeignKey("dbo.PickupLocation", t => t.PickupLocation_id)
                .Index(t => t.FoodBank_Id)
                .Index(t => t.Grower_Id)
                .Index(t => t.PickupLocation_id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Listing", "PickupLocation_id", "dbo.PickupLocation");
            DropForeignKey("dbo.Listing", "Grower_Id", "dbo.Grower");
            DropForeignKey("dbo.Listing", "FoodBank_Id", "dbo.FoodBank");
            DropForeignKey("dbo.PickupLocation", "Grower_Id", "dbo.Grower");
            DropForeignKey("dbo.DisplayMessage", "DisplayDescriptionId", "dbo.DisplayDescription");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Listing", new[] { "PickupLocation_id" });
            DropIndex("dbo.Listing", new[] { "Grower_Id" });
            DropIndex("dbo.Listing", new[] { "FoodBank_Id" });
            DropIndex("dbo.PickupLocation", new[] { "Grower_Id" });
            DropIndex("dbo.DisplayMessage", "DescriptionId_SortOrder_Unique");
            DropIndex("dbo.DisplayMessage", new[] { "DisplayDescriptionId" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Listing");
            DropTable("dbo.PickupLocation");
            DropTable("dbo.Grower");
            DropTable("dbo.FoodBank");
            DropTable("dbo.DisplayMessage");
            DropTable("dbo.DisplayDescription");
        }
    }
}
