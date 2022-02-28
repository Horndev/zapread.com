namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
