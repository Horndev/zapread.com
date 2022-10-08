namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserReactionUnlockAll : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Reaction", "UnlockedAll", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Reaction", "UnlockedAll");
        }
    }
}
