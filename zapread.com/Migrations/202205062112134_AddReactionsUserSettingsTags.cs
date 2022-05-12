namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddReactionsUserSettingsTags : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommentReaction",
                c => new
                    {
                        CommentReactionId = c.Int(nullable: false, identity: true),
                        Reaction_ReactionId = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.CommentReactionId)
                .ForeignKey("dbo.Reaction", t => t.Reaction_ReactionId)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Reaction_ReactionId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Reaction",
                c => new
                    {
                        ReactionId = c.Int(nullable: false, identity: true),
                        ReactionName = c.String(),
                        ReactionIcon = c.String(),
                        Image = c.Binary(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ReactionId);
            
            CreateTable(
                "dbo.PostReaction",
                c => new
                    {
                        PostReactionId = c.Int(nullable: false, identity: true),
                        Reaction_ReactionId = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.PostReactionId)
                .ForeignKey("dbo.Reaction", t => t.Reaction_ReactionId)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Reaction_ReactionId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        TagId = c.Int(nullable: false, identity: true),
                        TagName = c.String(),
                    })
                .PrimaryKey(t => t.TagId);
            
            CreateTable(
                "dbo.CommentCommentReaction",
                c => new
                    {
                        Comment_CommentId = c.Long(nullable: false),
                        CommentReaction_CommentReactionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Comment_CommentId, t.CommentReaction_CommentReactionId })
                .ForeignKey("dbo.Comment", t => t.Comment_CommentId, cascadeDelete: true)
                .ForeignKey("dbo.CommentReaction", t => t.CommentReaction_CommentReactionId, cascadeDelete: true)
                .Index(t => t.Comment_CommentId)
                .Index(t => t.CommentReaction_CommentReactionId);
            
            CreateTable(
                "dbo.PostPostReaction",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        PostReaction_PostReactionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.PostReaction_PostReactionId })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true)
                .ForeignKey("dbo.PostReaction", t => t.PostReaction_PostReactionId, cascadeDelete: true)
                .Index(t => t.Post_PostId)
                .Index(t => t.PostReaction_PostReactionId);
            
            CreateTable(
                "dbo.PostTag",
                c => new
                    {
                        Post_PostId = c.Int(nullable: false),
                        Tag_TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Post_PostId, t.Tag_TagId })
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true)
                .ForeignKey("dbo.Tag", t => t.Tag_TagId, cascadeDelete: true)
                .Index(t => t.Post_PostId)
                .Index(t => t.Tag_TagId);
            
            AddColumn("dbo.Post", "IsNonIncome", c => c.Boolean(nullable: false));
            AddColumn("dbo.Post", "LockComments", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "MailWeeklySummary", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "MailLoginEvent", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "MailTransactionEvent", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "MailMonthlyReport", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "MailNewsletter", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "ShowCommentsInFeed", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "AutoWithdraw", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "LockWithdraw", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserSettings", "TimeStampWithdrawUnlock", c => c.DateTime());
            AddColumn("dbo.UserSettings", "EncryptionKey", c => c.String());
            AddColumn("dbo.UserSettings", "EncryptionKey2", c => c.String());
            AddColumn("dbo.UserSettings", "PublicKey", c => c.String());
            AddColumn("dbo.UserSettings", "PublicKeyInfo", c => c.String());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PostTag", "Tag_TagId", "dbo.Tag");
            DropForeignKey("dbo.PostTag", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.PostPostReaction", "PostReaction_PostReactionId", "dbo.PostReaction");
            DropForeignKey("dbo.PostPostReaction", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.PostReaction", "User_Id", "dbo.User");
            DropForeignKey("dbo.PostReaction", "Reaction_ReactionId", "dbo.Reaction");
            DropForeignKey("dbo.CommentCommentReaction", "CommentReaction_CommentReactionId", "dbo.CommentReaction");
            DropForeignKey("dbo.CommentCommentReaction", "Comment_CommentId", "dbo.Comment");
            DropForeignKey("dbo.CommentReaction", "User_Id", "dbo.User");
            DropForeignKey("dbo.CommentReaction", "Reaction_ReactionId", "dbo.Reaction");
            DropIndex("dbo.PostTag", new[] { "Tag_TagId" });
            DropIndex("dbo.PostTag", new[] { "Post_PostId" });
            DropIndex("dbo.PostPostReaction", new[] { "PostReaction_PostReactionId" });
            DropIndex("dbo.PostPostReaction", new[] { "Post_PostId" });
            DropIndex("dbo.CommentCommentReaction", new[] { "CommentReaction_CommentReactionId" });
            DropIndex("dbo.CommentCommentReaction", new[] { "Comment_CommentId" });
            DropIndex("dbo.PostReaction", new[] { "User_Id" });
            DropIndex("dbo.PostReaction", new[] { "Reaction_ReactionId" });
            DropIndex("dbo.CommentReaction", new[] { "User_Id" });
            DropIndex("dbo.CommentReaction", new[] { "Reaction_ReactionId" });
            DropColumn("dbo.UserSettings", "PublicKeyInfo");
            DropColumn("dbo.UserSettings", "PublicKey");
            DropColumn("dbo.UserSettings", "EncryptionKey2");
            DropColumn("dbo.UserSettings", "EncryptionKey");
            DropColumn("dbo.UserSettings", "TimeStampWithdrawUnlock");
            DropColumn("dbo.UserSettings", "LockWithdraw");
            DropColumn("dbo.UserSettings", "AutoWithdraw");
            DropColumn("dbo.UserSettings", "ShowCommentsInFeed");
            DropColumn("dbo.UserSettings", "MailNewsletter");
            DropColumn("dbo.UserSettings", "MailMonthlyReport");
            DropColumn("dbo.UserSettings", "MailTransactionEvent");
            DropColumn("dbo.UserSettings", "MailLoginEvent");
            DropColumn("dbo.UserSettings", "MailWeeklySummary");
            DropColumn("dbo.Post", "LockComments");
            DropColumn("dbo.Post", "IsNonIncome");
            DropTable("dbo.PostTag");
            DropTable("dbo.PostPostReaction");
            DropTable("dbo.CommentCommentReaction");
            DropTable("dbo.Tag");
            DropTable("dbo.PostReaction");
            DropTable("dbo.Reaction");
            DropTable("dbo.CommentReaction");
        }
    }
}
