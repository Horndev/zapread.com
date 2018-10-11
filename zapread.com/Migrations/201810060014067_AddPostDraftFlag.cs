namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostDraftFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "IsDraft", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Post", "IsDraft");
        }
    }
}
