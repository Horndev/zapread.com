namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class expand_db_completeness : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserFunds",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TotalEarned = c.Double(nullable: false),
                    Balance = c.Double(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Comment", "TimeStamp", c => c.DateTime());
            AddColumn("dbo.User", "DateJoined", c => c.DateTime());
            AddColumn("dbo.User", "Funds_Id", c => c.Int());
            AddColumn("dbo.UserImage", "Post_PostId", c => c.Int());
            AddColumn("dbo.Group", "Earned", c => c.Double(nullable: false));
            CreateIndex("dbo.User", "Funds_Id");
            CreateIndex("dbo.UserImage", "Post_PostId");
            AddForeignKey("dbo.User", "Funds_Id", "dbo.UserFunds", "Id");
            AddForeignKey("dbo.UserImage", "Post_PostId", "dbo.Post", "PostId");
        }

        public override void Down()
        {
            DropForeignKey("dbo.UserImage", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.User", "Funds_Id", "dbo.UserFunds");
            DropIndex("dbo.UserImage", new[] { "Post_PostId" });
            DropIndex("dbo.User", new[] { "Funds_Id" });
            DropColumn("dbo.Group", "Earned");
            DropColumn("dbo.UserImage", "Post_PostId");
            DropColumn("dbo.User", "Funds_Id");
            DropColumn("dbo.User", "DateJoined");
            DropColumn("dbo.Comment", "TimeStamp");
            DropTable("dbo.UserFunds");
        }
    }
}
