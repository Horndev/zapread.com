namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserContentReports : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.UserContentReport",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsResolved = c.Boolean(nullable: false),
                        IsStarted = c.Boolean(nullable: false),
                        ReportType = c.Int(nullable: false),
                        TimeStamp = c.DateTime(),
                        ResolveDate = c.DateTime(),
                        ReportedBy_Id = c.Int(),
                        ResolvedBy_Id = c.Int(),
                        Post_PostId = c.Int(),
                        Comment_CommentId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.ReportedBy_Id)
                .ForeignKey("dbo.User", t => t.ResolvedBy_Id)
                .ForeignKey("dbo.Post", t => t.Post_PostId)
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId)
                .Index(t => t.ReportedBy_Id)
                .Index(t => t.ResolvedBy_Id)
                .Index(t => t.Post_PostId)
                .Index(t => t.Comment_CommentId);
            
            AddColumn("dbo.Group", "Funds_Id", c => c.Int());
            CreateIndex("dbo.Group", "Funds_Id");
            AddForeignKey("dbo.Group", "Funds_Id", "dbo.UserFunds", "Id");
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.UserContentReport", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.UserContentReport", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.UserContentReport", "ResolvedBy_Id", "dbo.User");
            DropForeignKey("dbo.UserContentReport", "ReportedBy_Id", "dbo.User");
            DropForeignKey("dbo.Group", "Funds_Id", "dbo.UserFunds");
            DropIndex("dbo.UserContentReport", new[] { "Comment_CommentId" });
            DropIndex("dbo.UserContentReport", new[] { "Post_PostId" });
            DropIndex("dbo.UserContentReport", new[] { "ResolvedBy_Id" });
            DropIndex("dbo.UserContentReport", new[] { "ReportedBy_Id" });
            DropIndex("dbo.Group", new[] { "Funds_Id" });
            DropColumn("dbo.Group", "Funds_Id");
            DropTable("dbo.UserContentReport");
        }
    }
}
