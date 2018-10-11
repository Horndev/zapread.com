namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostIsPublished : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "IsPublished", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Post", "IsPublished");
        }
    }
}
