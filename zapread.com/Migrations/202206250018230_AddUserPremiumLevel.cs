namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserPremiumLevel : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.User", "PremiumLevel", c => c.Int(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.User", "PremiumLevel");
        }
    }
}
