namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_User_AboutMe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "AboutMe", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "AboutMe");
        }
    }
}
