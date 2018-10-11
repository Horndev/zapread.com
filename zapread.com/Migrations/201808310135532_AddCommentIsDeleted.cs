namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentIsDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comment", "IsDeleted");
        }
    }
}
