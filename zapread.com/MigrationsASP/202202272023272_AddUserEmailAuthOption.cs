namespace zapread.com.MigrationsASP
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserEmailAuthOption : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsEmailAuthenticatorEnabled", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsEmailAuthenticatorEnabled");
        }
    }
}
