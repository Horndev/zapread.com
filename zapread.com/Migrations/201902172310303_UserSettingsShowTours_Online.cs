namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class UserSettingsShowTours_Online : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "ShowTours", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "ShowOnline", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "ShowOnline");
            DropColumn("dbo.UserSettings", "ShowTours");
        }
    }
}
