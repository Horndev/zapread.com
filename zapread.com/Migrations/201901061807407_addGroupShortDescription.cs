namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addGroupShortDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "ShortDescription", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Group", "ShortDescription");
        }
    }
}
