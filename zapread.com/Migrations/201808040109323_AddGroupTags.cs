namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
