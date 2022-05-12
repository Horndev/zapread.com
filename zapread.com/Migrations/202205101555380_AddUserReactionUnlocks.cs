namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserReactionUnlocks : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ReactionUser",
                c => new
                    {
                        Reaction_ReactionId = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Reaction_ReactionId, t.User_Id })
                .ForeignKey("dbo.Reaction", t => t.Reaction_ReactionId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Reaction_ReactionId)
                .Index(t => t.User_Id);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ReactionUser", "User_Id", "dbo.User");
            DropForeignKey("dbo.ReactionUser", "Reaction_ReactionId", "dbo.Reaction");
            DropIndex("dbo.ReactionUser", new[] { "User_Id" });
            DropIndex("dbo.ReactionUser", new[] { "Reaction_ReactionId" });
            DropTable("dbo.ReactionUser");
        }
    }
}
