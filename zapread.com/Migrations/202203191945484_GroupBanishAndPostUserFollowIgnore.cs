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
        /// NOTE: custom FK names used in migration since existing names existed for renamed tables
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
                    })
                .PrimaryKey(t => t.BanishedId);
            
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
                "dbo.GroupGroupBanished",
                c => new
                    {
                        Group_GroupId = c.Int(nullable: false),
                        GroupBanished_BanishedId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Group_GroupId, t.GroupBanished_BanishedId })
                .ForeignKey("dbo.Group", t => t.Group_GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupBanished", t => t.GroupBanished_BanishedId, cascadeDelete: true)
                .Index(t => t.Group_GroupId)
                .Index(t => t.GroupBanished_BanishedId);
            
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
            
            CreateTable(
                "dbo.UserGroupBanished",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        GroupBanished_BanishedId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.GroupBanished_BanishedId })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.GroupBanished", t => t.GroupBanished_BanishedId, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.GroupBanished_BanishedId);
            
            AddColumn("dbo.Group", "CustomTemplate", c => c.String());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.UserGroupBanished", "GroupBanished_BanishedId", "dbo.GroupBanished");
            DropForeignKey("dbo.UserGroupBanished", "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser1", name: "FK_dbo.PostUser1_dbo.Post_User_Id_220319");// "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser1", name: "FK_dbo.PostUser1_dbo.Post_Post_PostId_220319");// "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.GroupGroupBanished", "GroupBanished_BanishedId", "dbo.GroupBanished");
            DropForeignKey("dbo.GroupGroupBanished", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.PostUser", name: "FK_dbo.PostUser_dbo.Post_User_Id_220319");// "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser", name: "FK_dbo.PostUser_dbo.Post_Post_PostId_220319");// "Post_PostId", "dbo.Post");
            DropIndex("dbo.UserGroupBanished", new[] { "GroupBanished_BanishedId" });
            DropIndex("dbo.UserGroupBanished", new[] { "User_Id" });
            DropIndex("dbo.PostUser1", new[] { "User_Id" });
            DropIndex("dbo.PostUser1", new[] { "Post_PostId" });
            DropIndex("dbo.GroupGroupBanished", new[] { "GroupBanished_BanishedId" });
            DropIndex("dbo.GroupGroupBanished", new[] { "Group_GroupId" });
            DropIndex("dbo.PostUser", new[] { "User_Id" });
            DropIndex("dbo.PostUser", new[] { "Post_PostId" });
            DropColumn("dbo.Group", "CustomTemplate");
            DropTable("dbo.UserGroupBanished");
            DropTable("dbo.PostUser1");
            DropTable("dbo.GroupGroupBanished");
            DropTable("dbo.PostUser");
            DropTable("dbo.GroupBanished");
            RenameTable(name: "dbo.PostUser3", newName: "PostUser1");
            RenameTable(name: "dbo.PostUser2", newName: "PostUser");
        }
    }
}
