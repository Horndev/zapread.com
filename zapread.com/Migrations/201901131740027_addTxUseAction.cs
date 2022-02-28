namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class addTxUseAction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "UsedForAction", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "UsedForAction");
        }
    }
}
