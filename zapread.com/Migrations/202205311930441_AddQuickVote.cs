namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddQuickVote : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.UserFunds", "QuickVoteOn", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserFunds", "QuickVoteAmount", c => c.Int(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.UserFunds", "QuickVoteAmount");
            DropColumn("dbo.UserFunds", "QuickVoteOn");
        }
    }
}
