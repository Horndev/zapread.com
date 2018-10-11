namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Earning_Events : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EarningEvent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        OriginType = c.Int(nullable: false),
                        Amount = c.Double(nullable: false),
                        TimeStamp = c.DateTime(),
                        User_Id = c.Int(),
                        Post_PostId = c.Int(),
                        Comment_CommentId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.Post", t => t.Post_PostId)
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId)
                .Index(t => t.User_Id)
                .Index(t => t.Post_PostId)
                .Index(t => t.Comment_CommentId);
            
            AddColumn("dbo.Comment", "TotalEarned", c => c.Double(nullable: false));
            AddColumn("dbo.User", "TotalEarned", c => c.Double(nullable: false));
            AddColumn("dbo.Post", "TotalEarned", c => c.Double(nullable: false));
            DropColumn("dbo.Post", "Earned");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Post", "Earned", c => c.Double(nullable: false));
            DropForeignKey("dbo.EarningEvent", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.EarningEvent", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.EarningEvent", "User_Id", "dbo.User");
            DropIndex("dbo.EarningEvent", new[] { "Comment_CommentId" });
            DropIndex("dbo.EarningEvent", new[] { "Post_PostId" });
            DropIndex("dbo.EarningEvent", new[] { "User_Id" });
            DropColumn("dbo.Post", "TotalEarned");
            DropColumn("dbo.User", "TotalEarned");
            DropColumn("dbo.Comment", "TotalEarned");
            DropTable("dbo.EarningEvent");
        }
    }
}
