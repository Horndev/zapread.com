namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddStickyPost : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "IsSticky", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Post", "IsSticky");
        }
    }
}
