namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEarningOriginID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EarningEvent", "OriginId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EarningEvent", "OriginId");
        }
    }
}
