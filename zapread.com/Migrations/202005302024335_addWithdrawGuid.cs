namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
