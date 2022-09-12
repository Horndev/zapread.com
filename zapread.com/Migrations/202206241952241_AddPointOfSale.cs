namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddPointOfSale : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Provider = c.Int(nullable: false),
                        OrderId = c.String(),
                        LocationId = c.String(),
                        ItemId = c.String(),
                        Price = c.Double(nullable: false),
                        Currency = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Subscription",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Provider = c.Int(nullable: false),
                        SubscriptionId = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        ActiveDate = c.DateTime(),
                        LastChecked = c.DateTime(),
                        Plan_Id = c.Guid(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubscriptionPlan", t => t.Plan_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Plan_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.SubscriptionPayment",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Timestamp = c.DateTime(),
                        BTCPrice = c.Double(nullable: false),
                        BalanceAwarded = c.Double(nullable: false),
                        Subscription_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscription", t => t.Subscription_Id)
                .Index(t => t.Subscription_Id);
            
            CreateTable(
                "dbo.SubscriptionPlan",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Provider = c.Int(nullable: false),
                        PlanId = c.String(),
                        Name = c.String(),
                        Price = c.Double(nullable: false),
                        Currency = c.String(),
                        Cadence = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Customer",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Provider = c.Int(nullable: false),
                        CustomerId = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.UserFunds", "SpendOnlyBalance", c => c.Double(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Customer", "User_Id", "dbo.User");
            DropForeignKey("dbo.Subscription", "User_Id", "dbo.User");
            DropForeignKey("dbo.Subscription", "Plan_Id", "dbo.SubscriptionPlan");
            DropForeignKey("dbo.SubscriptionPayment", "Subscription_Id", "dbo.Subscription");
            DropForeignKey("dbo.Order", "User_Id", "dbo.User");
            DropIndex("dbo.Customer", new[] { "User_Id" });
            DropIndex("dbo.SubscriptionPayment", new[] { "Subscription_Id" });
            DropIndex("dbo.Subscription", new[] { "User_Id" });
            DropIndex("dbo.Subscription", new[] { "Plan_Id" });
            DropIndex("dbo.Order", new[] { "User_Id" });
            DropColumn("dbo.UserFunds", "SpendOnlyBalance");
            DropTable("dbo.Customer");
            DropTable("dbo.SubscriptionPlan");
            DropTable("dbo.SubscriptionPayment");
            DropTable("dbo.Subscription");
            DropTable("dbo.Order");
        }
    }
}
