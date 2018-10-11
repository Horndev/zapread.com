namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentScore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "Score", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comment", "Score");
        }
    }
}
