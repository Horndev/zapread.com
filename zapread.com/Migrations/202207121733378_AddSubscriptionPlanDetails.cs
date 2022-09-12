namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubscriptionPlanDetails : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.SubscriptionPlan", "Subtitle", c => c.String());
            AddColumn("dbo.SubscriptionPlan", "DescriptionHTML", c => c.String());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.SubscriptionPlan", "DescriptionHTML");
            DropColumn("dbo.SubscriptionPlan", "Subtitle");
        }
    }
}
