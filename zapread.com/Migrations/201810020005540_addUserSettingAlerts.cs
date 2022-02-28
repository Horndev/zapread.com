namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class addUserSettingAlerts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "NotifyOnMentioned", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnOwnPostCommented", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "AlertOnOwnPostCommented");
            DropColumn("dbo.UserSettings", "NotifyOnMentioned");
        }
    }
}
