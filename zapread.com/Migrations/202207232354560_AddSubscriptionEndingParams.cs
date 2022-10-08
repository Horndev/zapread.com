namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddSubscriptionEndingParams : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Subscription", "IsEnding", c => c.Boolean(nullable: false));
            AddColumn("dbo.Subscription", "EndDate", c => c.DateTime());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Subscription", "EndDate");
            DropColumn("dbo.Subscription", "IsEnding");
        }
    }
}
