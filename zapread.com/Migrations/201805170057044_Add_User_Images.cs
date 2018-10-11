namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_User_Images : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserImage",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        Image = c.Binary(),
                    })
                .PrimaryKey(t => t.ImageId);
            
            AddColumn("dbo.User", "ProfileImage_ImageId", c => c.Int());
            AddColumn("dbo.User", "ThumbImage_ImageId", c => c.Int());
            CreateIndex("dbo.User", "ProfileImage_ImageId");
            CreateIndex("dbo.User", "ThumbImage_ImageId");
            AddForeignKey("dbo.User", "ProfileImage_ImageId", "dbo.UserImage", "ImageId");
            AddForeignKey("dbo.User", "ThumbImage_ImageId", "dbo.UserImage", "ImageId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "ThumbImage_ImageId", "dbo.UserImage");
            DropForeignKey("dbo.User", "ProfileImage_ImageId", "dbo.UserImage");
            DropIndex("dbo.User", new[] { "ThumbImage_ImageId" });
            DropIndex("dbo.User", new[] { "ProfileImage_ImageId" });
            DropColumn("dbo.User", "ThumbImage_ImageId");
            DropColumn("dbo.User", "ProfileImage_ImageId");
            DropTable("dbo.UserImage");
        }
    }
}
