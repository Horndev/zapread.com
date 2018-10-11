namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialVersion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Comment",
                c => new
                    {
                        CommentId = c.Long(nullable: false, identity: true),
                        Text = c.String(),
                        UserId_Id = c.Int(),
                        Post_PostId = c.Int(),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.User", t => t.UserId_Id)
                .ForeignKey("dbo.Post", t => t.Post_PostId)
                .Index(t => t.UserId_Id)
                .Index(t => t.Post_PostId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        AppId = c.String(nullable: false),
                        Comment_CommentId = c.Long(),
                        Comment_CommentId1 = c.Long(),
                        Group_GroupId = c.Int(),
                        Group_GroupId1 = c.Int(),
                        Post_PostId = c.Int(),
                        Post_PostId1 = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId)
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId1)
                .ForeignKey("dbo.Group", t => t.Group_GroupId)
                .ForeignKey("dbo.Group", t => t.Group_GroupId1)
                .ForeignKey("dbo.Post", t => t.Post_PostId)
                .ForeignKey("dbo.Post", t => t.Post_PostId1)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.Comment_CommentId1)
                .Index(t => t.Group_GroupId)
                .Index(t => t.Group_GroupId1)
                .Index(t => t.Post_PostId)
                .Index(t => t.Post_PostId1);
            
            CreateTable(
                "dbo.Group",
                c => new
                    {
                        GroupId = c.Int(nullable: false, identity: true),
                        GroupName = c.String(),
                    })
                .PrimaryKey(t => t.GroupId);
            
            CreateTable(
                "dbo.Post",
                c => new
                    {
                        PostId = c.Int(nullable: false, identity: true),
                        Score = c.Int(nullable: false),
                        PostTitle = c.String(),
                        TimeStamp = c.DateTime(),
                        Content = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        Earned = c.Double(nullable: false),
                        Group_GroupId = c.Int(),
                        UserId_Id = c.Int(),
                    })
                .PrimaryKey(t => t.PostId)
                .ForeignKey("dbo.Group", t => t.Group_GroupId)
                .ForeignKey("dbo.User", t => t.UserId_Id)
                .Index(t => t.Group_GroupId)
                .Index(t => t.UserId_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "Post_PostId1", "dbo.Post");
            DropForeignKey("dbo.User", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.Post", "UserId_Id", "dbo.User");
            DropForeignKey("dbo.Post", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.Comment", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.User", "Group_GroupId1", "dbo.Group");
            DropForeignKey("dbo.User", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.User", "Comment_CommentId1", "dbo.Comment");
            DropForeignKey("dbo.User", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.Comment", "UserId_Id", "dbo.User");
            DropIndex("dbo.Post", new[] { "UserId_Id" });
            DropIndex("dbo.Post", new[] { "Group_GroupId" });
            DropIndex("dbo.User", new[] { "Post_PostId1" });
            DropIndex("dbo.User", new[] { "Post_PostId" });
            DropIndex("dbo.User", new[] { "Group_GroupId1" });
            DropIndex("dbo.User", new[] { "Group_GroupId" });
            DropIndex("dbo.User", new[] { "Comment_CommentId1" });
            DropIndex("dbo.User", new[] { "Comment_CommentId" });
            DropIndex("dbo.Comment", new[] { "Post_PostId" });
            DropIndex("dbo.Comment", new[] { "UserId_Id" });
            DropTable("dbo.Post");
            DropTable("dbo.Group");
            DropTable("dbo.User");
            DropTable("dbo.Comment");
        }
    }
}
