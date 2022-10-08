namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
