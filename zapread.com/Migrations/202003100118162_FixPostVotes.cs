namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixPostVotes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.User", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.User", "Post_PostId1", "dbo.Post");
            DropForeignKey("dbo.Post", "User_Id", "dbo.User");
            DropForeignKey("dbo.Post", "User_Id1", "dbo.User");
            DropIndex("dbo.User", new[] { "Post_PostId" });
            DropIndex("dbo.User", new[] { "Post_PostId1" });
            DropIndex("dbo.Post", new[] { "User_Id" });
            DropIndex("dbo.Post", new[] { "User_Id1" });
            CreateTable(
                "dbo.PostUser",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.User_Id })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Post_PostId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.PostUser1",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.User_Id })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Post_PostId)
                .Index(t => t.User_Id);
            
            DropColumn("dbo.User", "Post_PostId");
            DropColumn("dbo.User", "Post_PostId1");
            DropColumn("dbo.Post", "User_Id");
            DropColumn("dbo.Post", "User_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Post", "User_Id1", c => c.Int());
            AddColumn("dbo.Post", "User_Id", c => c.Int());
            AddColumn("dbo.User", "Post_PostId1", c => c.Int());
            AddColumn("dbo.User", "Post_PostId", c => c.Int());
            DropForeignKey("dbo.PostUser1", "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser1", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.PostUser", "User_Id", "dbo.User");
            DropForeignKey("dbo.PostUser", "Post_PostId", "dbo.Post");
            DropIndex("dbo.PostUser1", new[] { "User_Id" });
            DropIndex("dbo.PostUser1", new[] { "Post_PostId" });
            DropIndex("dbo.PostUser", new[] { "User_Id" });
            DropIndex("dbo.PostUser", new[] { "Post_PostId" });
            DropTable("dbo.PostUser1");
            DropTable("dbo.PostUser");
            CreateIndex("dbo.Post", "User_Id1");
            CreateIndex("dbo.Post", "User_Id");
            CreateIndex("dbo.User", "Post_PostId1");
            CreateIndex("dbo.User", "Post_PostId");
            AddForeignKey("dbo.Post", "User_Id1", "dbo.User", "Id");
            AddForeignKey("dbo.Post", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.User", "Post_PostId1", "dbo.Post", "PostId");
            AddForeignKey("dbo.User", "Post_PostId", "dbo.Post", "PostId");
        }
    }
}
