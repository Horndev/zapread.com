namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLimboBalance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserFunds", "LimboBalance", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserFunds", "LimboBalance");
        }
    }
}
