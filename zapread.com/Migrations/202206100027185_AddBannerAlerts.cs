namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddBannerAlerts : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BannerAlert",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Priority = c.String(),
                        IsDismissed = c.Boolean(nullable: false),
                        IsSnoozed = c.Boolean(nullable: false),
                        IsSticky = c.Boolean(nullable: false),
                        SnoozeTime = c.DateTime(),
                        StartTime = c.DateTime(),
                        DeleteTime = c.DateTime(),
                        TimeStamp = c.DateTime(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.BannerAlert", "User_Id", "dbo.User");
            DropIndex("dbo.BannerAlert", new[] { "User_Id" });
            DropTable("dbo.BannerAlert");
        }
    }
}
