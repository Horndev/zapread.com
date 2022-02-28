namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddUserSettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserSettings",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    NotifyOnOwnPostCommented = c.Boolean(nullable: false),
                    NotifyOnOwnCommentReplied = c.Boolean(nullable: false),
                    NotifyOnNewPostSubscribedGroup = c.Boolean(nullable: false),
                    NotifyOnNewPostSubscribedUser = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.User", "Settings_Id", c => c.Int());
            CreateIndex("dbo.User", "Settings_Id");
            AddForeignKey("dbo.User", "Settings_Id", "dbo.UserSettings", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.User", "Settings_Id", "dbo.UserSettings");
            DropIndex("dbo.User", new[] { "Settings_Id" });
            DropColumn("dbo.User", "Settings_Id");
            DropTable("dbo.UserSettings");
        }
    }
}
