namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddGroupCreationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "CreationDate", c => c.DateTime());
        }

        public override void Down()
        {
            DropColumn("dbo.Group", "CreationDate");
        }
    }
}
