namespace zapread.com.MigrationsASP
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsGoogleAuthenticatorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "GoogleAuthenticatorSecretKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "GoogleAuthenticatorSecretKey");
            DropColumn("dbo.AspNetUsers", "IsGoogleAuthenticatorEnabled");
        }
    }
}
