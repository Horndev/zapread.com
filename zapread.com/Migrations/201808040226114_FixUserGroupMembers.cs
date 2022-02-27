namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class FixUserGroupMembers : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.User", "IX_Group_GroupId");
            DropForeignKey("dbo.Group", "User_Id", "dbo.User");
            DropForeignKey("dbo.User", "Group_GroupId", "dbo.Group");
            DropIndex("dbo.Group", new[] { "User_Id" });
            DropIndex("dbo.User", new[] { "Group_GroupId1" });
            DropColumn("dbo.User", "Group_GroupId");
            RenameColumn(table: "dbo.User", name: "Group_GroupId1", newName: "Group_GroupId");
            CreateTable(
                "dbo.UserGroup",
                c => new
                {
                    User_Id = c.Int(nullable: false),
                    Group_GroupId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.User_Id, t.Group_GroupId })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Group", t => t.Group_GroupId, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Group_GroupId);

            DropColumn("dbo.Group", "User_Id");
        }

        public override void Down()
        {
            AddColumn("dbo.Group", "User_Id", c => c.Int());
            DropForeignKey("dbo.UserGroup", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.UserGroup", "User_Id", "dbo.User");
            DropIndex("dbo.UserGroup", new[] { "Group_GroupId" });
            DropIndex("dbo.UserGroup", new[] { "User_Id" });
            DropTable("dbo.UserGroup");
            RenameColumn(table: "dbo.User", name: "Group_GroupId", newName: "Group_GroupId1");
            AddColumn("dbo.User", "Group_GroupId", c => c.Int());
            CreateIndex("dbo.User", "Group_GroupId1");
            CreateIndex("dbo.Group", "User_Id");
            AddForeignKey("dbo.User", "Group_GroupId", "dbo.Group", "GroupId");
            AddForeignKey("dbo.Group", "User_Id", "dbo.User", "Id");
        }
    }
}
