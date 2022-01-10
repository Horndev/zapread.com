namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addGroupImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "GroupHeaderImage_ImageId", c => c.Int());
            AddColumn("dbo.Group", "GroupImage_ImageId", c => c.Int());
            CreateIndex("dbo.Group", "GroupHeaderImage_ImageId");
            CreateIndex("dbo.Group", "GroupImage_ImageId");
            AddForeignKey("dbo.Group", "GroupHeaderImage_ImageId", "dbo.UserImage", "ImageId");
            AddForeignKey("dbo.Group", "GroupImage_ImageId", "dbo.UserImage", "ImageId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Group", "GroupImage_ImageId", "dbo.UserImage");
            DropForeignKey("dbo.Group", "GroupHeaderImage_ImageId", "dbo.UserImage");
            DropIndex("dbo.Group", new[] { "GroupImage_ImageId" });
            DropIndex("dbo.Group", new[] { "GroupHeaderImage_ImageId" });
            DropColumn("dbo.Group", "GroupImage_ImageId");
            DropColumn("dbo.Group", "GroupHeaderImage_ImageId");
        }
    }
}
