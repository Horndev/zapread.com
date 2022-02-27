namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
