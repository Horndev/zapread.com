namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addColorThemeSetting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "ColorTheme", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserSettings", "ColorTheme");
        }
    }
}
