namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "Tier", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Group", "Tier");
        }
    }
}
