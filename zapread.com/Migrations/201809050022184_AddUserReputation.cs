namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddUserReputation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "Reputation", c => c.Long(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.User", "Reputation");
        }
    }
}
