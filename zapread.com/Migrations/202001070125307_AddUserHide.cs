namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddUserHide : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "HiddenBy_Id", c => c.Int());
            AddColumn("dbo.ZapIcon", "ImageSize", c => c.Int(nullable: false));
            AddColumn("dbo.ZapIcon", "Image", c => c.Binary());
            CreateIndex("dbo.Post", "HiddenBy_Id");
            AddForeignKey("dbo.Post", "HiddenBy_Id", "dbo.User", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Post", "HiddenBy_Id", "dbo.User");
            DropIndex("dbo.Post", new[] { "HiddenBy_Id" });
            DropColumn("dbo.ZapIcon", "Image");
            DropColumn("dbo.ZapIcon", "ImageSize");
            DropColumn("dbo.Post", "HiddenBy_Id");
        }
    }
}
