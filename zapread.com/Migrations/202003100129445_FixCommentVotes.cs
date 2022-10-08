namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class FixCommentVotes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.User", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.User", "Comment_CommentId1", "dbo.Comment");
            DropForeignKey("dbo.Comment", "User_Id", "dbo.User");
            DropForeignKey("dbo.Comment", "User_Id1", "dbo.User");
            DropIndex("dbo.User", new[] { "Comment_CommentId" });
            DropIndex("dbo.User", new[] { "Comment_CommentId1" });
            DropIndex("dbo.Comment", new[] { "User_Id" });
            DropIndex("dbo.Comment", new[] { "User_Id1" });
            CreateTable(
                "dbo.CommentUser",
                c => new
                    {
                        Comment_CommentId = c.Long(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Comment_CommentId, t.User_Id })
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.CommentUser1",
                c => new
                    {
                        Comment_CommentId = c.Long(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Comment_CommentId, t.User_Id })
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.User_Id);
            
            DropColumn("dbo.User", "Comment_CommentId");
            DropColumn("dbo.User", "Comment_CommentId1");
            DropColumn("dbo.Comment", "User_Id");
            DropColumn("dbo.Comment", "User_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comment", "User_Id1", c => c.Int());
            AddColumn("dbo.Comment", "User_Id", c => c.Int());
            AddColumn("dbo.User", "Comment_CommentId1", c => c.Long());
            AddColumn("dbo.User", "Comment_CommentId", c => c.Long());
            DropForeignKey("dbo.CommentUser1", "User_Id", "dbo.User");
            DropForeignKey("dbo.CommentUser1", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.CommentUser", "User_Id", "dbo.User");
            DropForeignKey("dbo.CommentUser", "Comment_CommentId", "dbo.Comment");
            DropIndex("dbo.CommentUser1", new[] { "User_Id" });
            DropIndex("dbo.CommentUser1", new[] { "Comment_CommentId" });
            DropIndex("dbo.CommentUser", new[] { "User_Id" });
            DropIndex("dbo.CommentUser", new[] { "Comment_CommentId" });
            DropTable("dbo.CommentUser1");
            DropTable("dbo.CommentUser");
            CreateIndex("dbo.Comment", "User_Id1");
            CreateIndex("dbo.Comment", "User_Id");
            CreateIndex("dbo.User", "Comment_CommentId1");
            CreateIndex("dbo.User", "Comment_CommentId");
            AddForeignKey("dbo.Comment", "User_Id1", "dbo.User", "Id");
            AddForeignKey("dbo.Comment", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.User", "Comment_CommentId1", "dbo.Comment", "CommentId");
            AddForeignKey("dbo.User", "Comment_CommentId", "dbo.Comment", "CommentId");
        }
    }
}
