namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class addAppIdIndex : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.User", "AppId", c => c.String(nullable: false, maxLength: 37, unicode: false));
            CreateIndex("dbo.User", "AppId");
        }

        public override void Down()
        {
            DropIndex("dbo.User", new[] { "AppId" });
            AlterColumn("dbo.User", "AppId", c => c.String(nullable: false));
        }
    }
}
