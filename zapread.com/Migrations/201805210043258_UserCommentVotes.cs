namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UserCommentVotes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "User_Id", c => c.Int());
            AddColumn("dbo.Comment", "User_Id1", c => c.Int());
            CreateIndex("dbo.Comment", "User_Id");
            CreateIndex("dbo.Comment", "User_Id1");
            AddForeignKey("dbo.Comment", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.Comment", "User_Id1", "dbo.User", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Comment", "User_Id1", "dbo.User");
            DropForeignKey("dbo.Comment", "User_Id", "dbo.User");
            DropIndex("dbo.Comment", new[] { "User_Id1" });
            DropIndex("dbo.Comment", new[] { "User_Id" });
            DropColumn("dbo.Comment", "User_Id1");
            DropColumn("dbo.Comment", "User_Id");
        }
    }
}
