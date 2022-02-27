namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class UserImageAttr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserImage", "ContentType", c => c.String());
            AddColumn("dbo.UserImage", "XSize", c => c.Int(nullable: false));
            AddColumn("dbo.UserImage", "YSize", c => c.Int(nullable: false));
            AddColumn("dbo.UserImage", "UserAppId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserImage", "UserAppId");
            DropColumn("dbo.UserImage", "YSize");
            DropColumn("dbo.UserImage", "XSize");
            DropColumn("dbo.UserImage", "ContentType");
        }
    }
}
