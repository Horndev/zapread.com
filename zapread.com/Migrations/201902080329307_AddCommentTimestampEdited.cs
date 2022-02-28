namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddCommentTimestampEdited : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "TimeStampEdited", c => c.DateTime());
        }

        public override void Down()
        {
            DropColumn("dbo.Comment", "TimeStampEdited");
        }
    }
}
