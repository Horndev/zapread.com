namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class global_earning_spending : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EarningEvent", "ZapReadGlobals_Id", c => c.Int());
            AddColumn("dbo.SpendingEvent", "ZapReadGlobals_Id", c => c.Int());
            CreateIndex("dbo.EarningEvent", "ZapReadGlobals_Id");
            CreateIndex("dbo.SpendingEvent", "ZapReadGlobals_Id");
            AddForeignKey("dbo.EarningEvent", "ZapReadGlobals_Id", "dbo.ZapReadGlobals", "Id");
            AddForeignKey("dbo.SpendingEvent", "ZapReadGlobals_Id", "dbo.ZapReadGlobals", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SpendingEvent", "ZapReadGlobals_Id", "dbo.ZapReadGlobals");
            DropForeignKey("dbo.EarningEvent", "ZapReadGlobals_Id", "dbo.ZapReadGlobals");
            DropIndex("dbo.SpendingEvent", new[] { "ZapReadGlobals_Id" });
            DropIndex("dbo.EarningEvent", new[] { "ZapReadGlobals_Id" });
            DropColumn("dbo.SpendingEvent", "ZapReadGlobals_Id");
            DropColumn("dbo.EarningEvent", "ZapReadGlobals_Id");
        }
    }
}
