namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddFundsLock : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FundsLock",
                c => new
                    {
                        FundsLockId = c.Int(nullable: false, identity: true),
                        UserFundId = c.Int(nullable: false),
                        Reason = c.Int(nullable: false),
                        WithdrawLocked = c.Boolean(nullable: false),
                        DepositLocked = c.Boolean(nullable: false),
                        TransferLocked = c.Boolean(nullable: false),
                        SpendLocked = c.Boolean(nullable: false),
                        Description = c.String(),
                        TimeStampStarted = c.DateTime(),
                        TimeStampExpired = c.DateTime(),
                    })
                .PrimaryKey(t => t.FundsLockId)
                .ForeignKey("dbo.UserFunds", t => t.UserFundId, cascadeDelete: true)
                .Index(t => t.UserFundId);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FundsLock", "UserFundId", "dbo.UserFunds");
            DropIndex("dbo.FundsLock", new[] { "UserFundId" });
            DropTable("dbo.FundsLock");
        }
    }
}
