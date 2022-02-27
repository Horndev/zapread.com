namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class VotePostM2M : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "User_Id", c => c.Int());
            AddColumn("dbo.Post", "User_Id1", c => c.Int());
            CreateIndex("dbo.Post", "User_Id");
            CreateIndex("dbo.Post", "User_Id1");
            AddForeignKey("dbo.Post", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.Post", "User_Id1", "dbo.User", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Post", "User_Id1", "dbo.User");
            DropForeignKey("dbo.Post", "User_Id", "dbo.User");
            DropIndex("dbo.Post", new[] { "User_Id1" });
            DropIndex("dbo.Post", new[] { "User_Id" });
            DropColumn("dbo.Post", "User_Id1");
            DropColumn("dbo.Post", "User_Id");
        }
    }
}
