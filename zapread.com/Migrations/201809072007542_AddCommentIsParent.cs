namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddCommentIsParent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "IsReply", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Comment", "IsReply");
        }
    }
}
