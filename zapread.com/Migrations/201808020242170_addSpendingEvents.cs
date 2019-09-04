namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addSpendingEvents : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SpendingEvent",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TimeStamp = c.DateTime(),
                    Amount = c.Double(nullable: false),
                    Comment_CommentId = c.Long(),
                    Group_GroupId = c.Int(),
                    Post_PostId = c.Int(),
                    User_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId)
                .ForeignKey("dbo.Group", t => t.Group_GroupId)
                .ForeignKey("dbo.Post", t => t.Post_PostId)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.Group_GroupId)
                .Index(t => t.Post_PostId)
                .Index(t => t.User_Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.SpendingEvent", "User_Id", "dbo.User");
            DropForeignKey("dbo.SpendingEvent", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.SpendingEvent", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.SpendingEvent", "Comment_CommentId", "dbo.Comment");
            DropIndex("dbo.SpendingEvent", new[] { "User_Id" });
            DropIndex("dbo.SpendingEvent", new[] { "Post_PostId" });
            DropIndex("dbo.SpendingEvent", new[] { "Group_GroupId" });
            DropIndex("dbo.SpendingEvent", new[] { "Comment_CommentId" });
            DropTable("dbo.SpendingEvent");
        }
    }
}
