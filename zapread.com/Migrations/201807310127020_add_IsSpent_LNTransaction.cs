namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_IsSpent_LNTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "IsSpent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "IsSpent");
        }
    }
}
