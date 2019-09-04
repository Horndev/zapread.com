namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddGroupTags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "Tags", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Group", "Tags");
        }
    }
}
