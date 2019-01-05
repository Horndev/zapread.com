namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserDateLastActivity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DateLastActivity", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "DateLastActivity");
        }
    }
}
