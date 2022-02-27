namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddLanguages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "Language", c => c.String());
            AddColumn("dbo.User", "Languages", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.User", "Languages");
            DropColumn("dbo.Post", "Language");
        }
    }
}
