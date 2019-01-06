namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixUserIgnores : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserUser1",
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
            DropForeignKey("dbo.UserUser1", "User_Id1", "dbo.User");
            DropForeignKey("dbo.UserUser1", "User_Id", "dbo.User");
            DropIndex("dbo.UserUser1", new[] { "User_Id1" });
            DropIndex("dbo.UserUser1", new[] { "User_Id" });
            DropTable("dbo.UserUser1");
        }
    }
}
