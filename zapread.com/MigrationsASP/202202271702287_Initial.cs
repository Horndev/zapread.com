namespace zapread.com.MigrationsASP
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class Initial : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsGoogleAuthenticatorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "GoogleAuthenticatorSecretKey", c => c.String());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "GoogleAuthenticatorSecretKey");
            DropColumn("dbo.AspNetUsers", "IsGoogleAuthenticatorEnabled");
        }
    }
}
