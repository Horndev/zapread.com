namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLNTransactionUsedFor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "UsedFor", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "UsedFor");
        }
    }
}
