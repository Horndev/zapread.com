namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class GroupBanishAndPostUserFollowIgnore : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.PostUser", newName: "PostUser2");
            RenameTable(name: "dbo.PostUser1", newName: "PostUser3");
            CreateTable(
                "dbo.GroupBanished",
                c => new
                    {
                        BanishedId = c.Int(nullable: false, identity: true),
                        Reason = c.String(),
                        BanishmentType = c.Int(nullable: false),
                        TimeStampStarted = c.DateTime(),
                        TimeStampExpired = c.DateTime(),
                        Group_GroupId = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.BanishedId)
                .ForeignKey("dbo.Group", t => t.Group_GroupId)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Group_GroupId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.PostUser",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.User_Id })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true, name: "FK_dbo.PostUser_dbo.Post_Post_PostId_220319")
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true, name: "FK_dbo.PostUser_dbo.Post_User_Id_220319")
                .Index(t => t.Post_PostId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.PostUser1",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.User_Id })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true, name: "FK_dbo.PostUser1_dbo.Post_Post_PostId_220319")
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true, name: "FK_dbo.PostUser1_dbo.Post_User_Id_220319")
                .Index(t => t.Post_PostId)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.Group", "CustomTemplate", c => c.String());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupBanished", "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser1", name: "FK_dbo.PostUser1_dbo.Post_User_Id_220319");// "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser1", name: "FK_dbo.PostUser1_dbo.Post_Post_PostId_220319");// "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.GroupBanished", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.PostUser", name: "FK_dbo.PostUser_dbo.Post_User_Id_220319");// "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser", name: "FK_dbo.PostUser_dbo.Post_Post_PostId_220319");// "Post_PostId", "dbo.Post");
            DropIndex("dbo.PostUser1", new[] { "User_Id" });
            DropIndex("dbo.PostUser1", new[] { "Post_PostId" });
            DropIndex("dbo.PostUser", new[] { "User_Id" });
            DropIndex("dbo.PostUser", new[] { "Post_PostId" });
            DropIndex("dbo.GroupBanished", new[] { "User_Id" });
            DropIndex("dbo.GroupBanished", new[] { "Group_GroupId" });
            DropColumn("dbo.Group", "CustomTemplate");
            DropTable("dbo.PostUser1");
            DropTable("dbo.PostUser");
            DropTable("dbo.GroupBanished");
            RenameTable(name: "dbo.PostUser3", newName: "PostUser1");
            RenameTable(name: "dbo.PostUser2", newName: "PostUser");
        }
    }
}
