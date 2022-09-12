namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddBetaTesting : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BetaTest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        IsDisabled = c.Boolean(nullable: false),
                        StartTime = c.DateTime(),
                        StopTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserBetaTest",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        BetaTest_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.BetaTest_Id })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.BetaTest", t => t.BetaTest_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.BetaTest_Id);
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.UserBetaTest", "BetaTest_Id", "dbo.BetaTest");
            DropForeignKey("dbo.UserBetaTest", "User_Id", "dbo.User");
            DropIndex("dbo.UserBetaTest", new[] { "BetaTest_Id" });
            DropIndex("dbo.UserBetaTest", new[] { "User_Id" });
            DropTable("dbo.UserBetaTest");
            DropTable("dbo.BetaTest");
        }
    }
}
