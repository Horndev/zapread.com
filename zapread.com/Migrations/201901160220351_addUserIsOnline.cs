namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addUserIsOnline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "IsOnline", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.User", "IsOnline");
        }
    }
}
