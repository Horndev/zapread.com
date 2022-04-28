namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class UserAddBlocking : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.UserUser1", newName: "UserUser2");
            RenameTable(name: "dbo.UserUser", newName: "UserUser1");
            CreateTable(
                "dbo.UserUser",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        User_Id1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.User_Id1 })
                .ForeignKey("dbo.User", t => t.User_Id, name: "FK_dbo.UserUser_dbo.User_User_Id_220427")
                .ForeignKey("dbo.User", t => t.User_Id1, name: "FK_dbo.UserUser_dbo.User_User_Id1_220427")
                .Index(t => t.User_Id)
                .Index(t => t.User_Id1);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.UserUser", name: "FK_dbo.UserUser_dbo.User_User_Id_220427");
            DropForeignKey("dbo.UserUser", name: "FK_dbo.UserUser_dbo.User_User_Id1_220427");
            DropIndex("dbo.UserUser", new[] { "User_Id1" });
            DropIndex("dbo.UserUser", new[] { "User_Id" });
            DropTable("dbo.UserUser");
            RenameTable(name: "dbo.UserUser1", newName: "UserUser");
            RenameTable(name: "dbo.UserUser2", newName: "UserUser1");
        }
    }
}
