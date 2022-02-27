namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddLimboBalance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserFunds", "LimboBalance", c => c.Double(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.UserFunds", "LimboBalance");
        }
    }
}
