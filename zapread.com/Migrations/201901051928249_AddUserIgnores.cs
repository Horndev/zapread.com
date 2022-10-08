namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddUserIgnores : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserIgnoreUser",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.User", "UserIgnores_Id", c => c.Int());
            CreateIndex("dbo.User", "UserIgnores_Id");
            AddForeignKey("dbo.User", "UserIgnores_Id", "dbo.UserIgnoreUser", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.User", "UserIgnores_Id", "dbo.UserIgnoreUser");
            DropIndex("dbo.User", new[] { "UserIgnores_Id" });
            DropColumn("dbo.User", "UserIgnores_Id");
            DropTable("dbo.UserIgnoreUser");
        }
    }
}
