namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddPostImpressions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "Impressions", c => c.Long(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Post", "Impressions");
        }
    }
}
