namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddViewTranslatedLanguagesUserSetting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "ViewTranslatedLanguages", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserSettings", "ViewTranslatedLanguages");
        }
    }
}
