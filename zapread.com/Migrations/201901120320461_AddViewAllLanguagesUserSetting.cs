namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddViewAllLanguagesUserSetting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "ViewAllLanguages", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "ViewAllLanguages");
        }
    }
}
