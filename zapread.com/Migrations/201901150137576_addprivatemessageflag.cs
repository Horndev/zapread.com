namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprivatemessageflag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserMessage", "IsPrivateMessage", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserMessage", "IsPrivateMessage");
        }
    }
}
