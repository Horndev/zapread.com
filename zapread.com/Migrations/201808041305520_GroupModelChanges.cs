namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class GroupModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "Tier", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Group", "Tier");
        }
    }
}
