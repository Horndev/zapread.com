namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class addUserImageVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserImage", "Version", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserImage", "Version");
        }
    }
}
