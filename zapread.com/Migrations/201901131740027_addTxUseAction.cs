namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTxUseAction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "UsedForAction", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "UsedForAction");
        }
    }
}
