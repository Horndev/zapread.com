namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserReputation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "Reputation", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "Reputation");
        }
    }
}
