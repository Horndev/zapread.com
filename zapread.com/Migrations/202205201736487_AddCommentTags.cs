namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddCommentTags : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommentTag",
                c => new
                    {
                        Comment_CommentId = c.Long(nullable: false),
                        Tag_TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Comment_CommentId, t.Tag_TagId })
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId, cascadeDelete: true)
                .ForeignKey("dbo.Tag", t => t.Tag_TagId, cascadeDelete: true)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.Tag_TagId);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CommentTag", "Tag_TagId", "dbo.Tag");
            DropForeignKey("dbo.CommentTag", "Comment_CommentId", "dbo.Comment");
            DropIndex("dbo.CommentTag", new[] { "Tag_TagId" });
            DropIndex("dbo.CommentTag", new[] { "Comment_CommentId" });
            DropTable("dbo.CommentTag");
        }
    }
}
