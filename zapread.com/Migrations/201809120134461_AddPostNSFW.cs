namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddPostNSFW : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "IsNSFW", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Post", "IsNSFW");
        }
    }
}
