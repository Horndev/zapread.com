namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
