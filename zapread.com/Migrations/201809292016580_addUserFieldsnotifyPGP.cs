namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addUserFieldsnotifyPGP : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "PGPPubKey", c => c.String());
            AddColumn("dbo.UserSettings", "NotifyOnReceivedTip", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "NotifyOnPrivateMessage", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "NotifyOnPrivateMessage");
            DropColumn("dbo.UserSettings", "NotifyOnReceivedTip");
            DropColumn("dbo.User", "PGPPubKey");
        }
    }
}
