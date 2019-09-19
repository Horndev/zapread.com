namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Achievements : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAchievement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateAchieved = c.DateTime(),
                        Achievement_Id = c.Int(),
                        AchievedBy_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Achievement", t => t.Achievement_Id)
                .ForeignKey("dbo.User", t => t.AchievedBy_Id)
                .Index(t => t.Achievement_Id)
                .Index(t => t.AchievedBy_Id);
            
            CreateTable(
                "dbo.Achievement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Image = c.Binary(),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAchievement", "AchievedBy_Id", "dbo.User");
            DropForeignKey("dbo.UserAchievement", "Achievement_Id", "dbo.Achievement");
            DropIndex("dbo.UserAchievement", new[] { "AchievedBy_Id" });
            DropIndex("dbo.UserAchievement", new[] { "Achievement_Id" });
            DropTable("dbo.Achievement");
            DropTable("dbo.UserAchievement");
        }
    }
}
