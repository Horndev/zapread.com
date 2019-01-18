namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserSettingCollapseDiscussions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "CollapseDiscussions", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserSettings", "CollapseDiscussions");
        }
    }
}
