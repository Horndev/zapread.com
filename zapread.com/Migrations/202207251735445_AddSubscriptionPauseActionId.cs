namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddSubscriptionPauseActionId : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Subscription", "PauseActionId", c => c.String());
            AddColumn("dbo.Subscription", "PauseDate", c => c.DateTime());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Subscription", "PauseDate");
            DropColumn("dbo.Subscription", "PauseActionId");
        }
    }
}
