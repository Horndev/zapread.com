namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class UpdateBannerAlertsGlobal : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BannerAlertUser",
                c => new
                    {
                        BannerAlert_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BannerAlert_Id, t.User_Id })
                .ForeignKey("dbo.BannerAlert", t => t.BannerAlert_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.BannerAlert_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.BannerAlert", "Title", c => c.String());
            AddColumn("dbo.BannerAlert", "IsGlobalSend", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.BannerAlertUser", "User_Id", "dbo.User");
            DropForeignKey("dbo.BannerAlertUser", "BannerAlert_Id", "dbo.BannerAlert");
            DropIndex("dbo.BannerAlertUser", new[] { "User_Id" });
            DropIndex("dbo.BannerAlertUser", new[] { "BannerAlert_Id" });
            DropColumn("dbo.BannerAlert", "IsGlobalSend");
            DropColumn("dbo.BannerAlert", "Title");
            DropTable("dbo.BannerAlertUser");
        }
    }
}
