namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addAlertsForAllNotify : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "AlertOnOwnCommentReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnNewPostSubscribedGroup", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnNewPostSubscribedUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnReceivedTip", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnPrivateMessage", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AlertOnMentioned", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "AlertOnMentioned");
            DropColumn("dbo.UserSettings", "AlertOnPrivateMessage");
            DropColumn("dbo.UserSettings", "AlertOnReceivedTip");
            DropColumn("dbo.UserSettings", "AlertOnNewPostSubscribedUser");
            DropColumn("dbo.UserSettings", "AlertOnNewPostSubscribedGroup");
            DropColumn("dbo.UserSettings", "AlertOnOwnCommentReplied");
        }
    }
}
