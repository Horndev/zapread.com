namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
