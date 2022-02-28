namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class addInverseGroupMember : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserGroup", newName: "GroupUser");
            DropPrimaryKey("dbo.GroupUser");
            AddPrimaryKey("dbo.GroupUser", new[] { "Group_GroupId", "User_Id" });
        }

        public override void Down()
        {
            DropPrimaryKey("dbo.GroupUser");
            AddPrimaryKey("dbo.GroupUser", new[] { "User_Id", "Group_GroupId" });
            RenameTable(name: "dbo.GroupUser", newName: "UserGroup");
        }
    }
}
