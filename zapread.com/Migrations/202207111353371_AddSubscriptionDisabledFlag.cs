namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddSubscriptionDisabledFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubscriptionPlan", "IsDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubscriptionPlan", "IsDisabled");
        }
    }
}
