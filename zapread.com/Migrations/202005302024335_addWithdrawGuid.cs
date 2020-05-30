namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addWithdrawGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "WithdrawId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "WithdrawId");
        }
    }
}
