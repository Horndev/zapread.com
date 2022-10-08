namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddUserReactionTimestamps : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.CommentReaction", "TimeStamp", c => c.DateTime());
            AddColumn("dbo.PostReaction", "TimeStamp", c => c.DateTime());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.PostReaction", "TimeStamp");
            DropColumn("dbo.CommentReaction", "TimeStamp");
        }
    }
}
