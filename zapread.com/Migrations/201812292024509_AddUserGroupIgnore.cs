namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddUserGroupIgnore : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.GroupUser2", newName: "GroupUser3");
            RenameTable(name: "dbo.GroupUser1", newName: "GroupUser2");
            CreateTable(
                "dbo.GroupUser1",
                c => new
                {
                    Group_GroupId = c.Int(nullable: false),
                    User_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Group_GroupId, t.User_Id })
                .ForeignKey("dbo.Group", t => t.Group_GroupId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Group_GroupId)
                .Index(t => t.User_Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.GroupUser1", "User_Id", "dbo.User");
            DropForeignKey("dbo.GroupUser1", "Group_GroupId", "dbo.Group");
            DropIndex("dbo.GroupUser1", new[] { "User_Id" });
            DropIndex("dbo.GroupUser1", new[] { "Group_GroupId" });
            DropTable("dbo.GroupUser1");
            RenameTable(name: "dbo.GroupUser2", newName: "GroupUser1");
            RenameTable(name: "dbo.GroupUser3", newName: "GroupUser2");
        }
    }
}
