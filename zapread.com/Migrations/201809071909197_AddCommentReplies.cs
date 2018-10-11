namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentReplies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "Parent_CommentId", c => c.Long());
            CreateIndex("dbo.Comment", "Parent_CommentId");
            AddForeignKey("dbo.Comment", "Parent_CommentId", "dbo.Comment", "CommentId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comment", "Parent_CommentId", "dbo.Comment");
            DropIndex("dbo.Comment", new[] { "Parent_CommentId" });
            DropColumn("dbo.Comment", "Parent_CommentId");
        }
    }
}
