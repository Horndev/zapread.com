namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddZapIconsDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ZapIcon",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Icon = c.String(maxLength: 80),
                    NumUses = c.Int(nullable: false),
                    Lib = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Icon, unique: true);

        }

        public override void Down()
        {
            DropIndex("dbo.ZapIcon", new[] { "Icon" });
            DropTable("dbo.ZapIcon");
        }
    }
}
