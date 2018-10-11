namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserMessages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserMessage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        TimeStamp = c.DateTime(),
                        IsRead = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CommentLink_CommentId = c.Long(),
                        From_Id = c.Int(),
                        PostLink_PostId = c.Int(),
                        To_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comment", t => t.CommentLink_CommentId)
                .ForeignKey("dbo.User", t => t.From_Id)
                .ForeignKey("dbo.Post", t => t.PostLink_PostId)
                .ForeignKey("dbo.User", t => t.To_Id)
                .Index(t => t.CommentLink_CommentId)
                .Index(t => t.From_Id)
                .Index(t => t.PostLink_PostId)
                .Index(t => t.To_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserMessage", "To_Id", "dbo.User");
            DropForeignKey("dbo.UserMessage", "PostLink_PostId", "dbo.Post");
            DropForeignKey("dbo.UserMessage", "From_Id", "dbo.User");
            DropForeignKey("dbo.UserMessage", "CommentLink_CommentId", "dbo.Comment");
            DropIndex("dbo.UserMessage", new[] { "To_Id" });
            DropIndex("dbo.UserMessage", new[] { "PostLink_PostId" });
            DropIndex("dbo.UserMessage", new[] { "From_Id" });
            DropIndex("dbo.UserMessage", new[] { "CommentLink_CommentId" });
            DropTable("dbo.UserMessage");
        }
    }
}
