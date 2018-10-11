namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserFollowing : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserUser",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        User_Id1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.User_Id1 })
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.User", t => t.User_Id1)
                .Index(t => t.User_Id)
                .Index(t => t.User_Id1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserUser", "User_Id1", "dbo.User");
            DropForeignKey("dbo.UserUser", "User_Id", "dbo.User");
            DropIndex("dbo.UserUser", new[] { "User_Id1" });
            DropIndex("dbo.UserUser", new[] { "User_Id" });
            DropTable("dbo.UserUser");
        }
    }
}
