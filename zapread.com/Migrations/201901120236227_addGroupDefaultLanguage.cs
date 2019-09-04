namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addGroupDefaultLanguage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "DefaultLanguage", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Group", "DefaultLanguage");
        }
    }
}
