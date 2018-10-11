namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingVoteTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PendingCommentVote",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CommentId = c.Int(nullable: false),
                        Direction = c.Int(nullable: false),
                        IsComplete = c.Boolean(nullable: false),
                        Payment_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LNTransaction", t => t.Payment_Id)
                .Index(t => t.Payment_Id);
            
            CreateTable(
                "dbo.PendingPostVote",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        Direction = c.Int(nullable: false),
                        IsComplete = c.Boolean(nullable: false),
                        Payment_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LNTransaction", t => t.Payment_Id)
                .Index(t => t.Payment_Id);
            
            AddColumn("dbo.LNTransaction", "UsedForId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PendingPostVote", "Payment_Id", "dbo.LNTransaction");
            DropForeignKey("dbo.PendingCommentVote", "Payment_Id", "dbo.LNTransaction");
            DropIndex("dbo.PendingPostVote", new[] { "Payment_Id" });
            DropIndex("dbo.PendingCommentVote", new[] { "Payment_Id" });
            DropColumn("dbo.LNTransaction", "UsedForId");
            DropTable("dbo.PendingPostVote");
            DropTable("dbo.PendingCommentVote");
        }
    }
}
