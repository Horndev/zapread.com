namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class UsersHaveGroups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "User_Id", c => c.Int());
            CreateIndex("dbo.Group", "User_Id");
            AddForeignKey("dbo.Group", "User_Id", "dbo.User", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Group", "User_Id", "dbo.User");
            DropIndex("dbo.Group", new[] { "User_Id" });
            DropColumn("dbo.Group", "User_Id");
        }
    }
}
