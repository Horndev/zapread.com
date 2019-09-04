namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddUserAlerts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAlert",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Title = c.String(),
                    Content = c.String(),
                    TimeStamp = c.DateTime(),
                    IsRead = c.Boolean(nullable: false),
                    IsDeleted = c.Boolean(nullable: false),
                    CommentLink_CommentId = c.Long(),
                    PostLink_PostId = c.Int(),
                    To_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comment", t => t.CommentLink_CommentId)
                .ForeignKey("dbo.Post", t => t.PostLink_PostId)
                .ForeignKey("dbo.User", t => t.To_Id)
                .Index(t => t.CommentLink_CommentId)
                .Index(t => t.PostLink_PostId)
                .Index(t => t.To_Id);

            AddColumn("dbo.UserMessage", "Title", c => c.String());
        }

        public override void Down()
        {
            DropForeignKey("dbo.UserAlert", "To_Id", "dbo.User");
            DropForeignKey("dbo.UserAlert", "PostLink_PostId", "dbo.Post");
            DropForeignKey("dbo.UserAlert", "CommentLink_CommentId", "dbo.Comment");
            DropIndex("dbo.UserAlert", new[] { "To_Id" });
            DropIndex("dbo.UserAlert", new[] { "PostLink_PostId" });
            DropIndex("dbo.UserAlert", new[] { "CommentLink_CommentId" });
            DropColumn("dbo.UserMessage", "Title");
            DropTable("dbo.UserAlert");
        }
    }
}
