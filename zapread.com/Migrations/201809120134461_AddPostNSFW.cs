namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostNSFW : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "IsNSFW", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Post", "IsNSFW");
        }
    }
}
