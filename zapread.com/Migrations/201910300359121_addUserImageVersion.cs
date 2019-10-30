namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserImageVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserImage", "Version", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserImage", "Version");
        }
    }
}
