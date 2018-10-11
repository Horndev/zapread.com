namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupAdmin : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.User", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.User", "Group_GroupId1", "dbo.Group");
            AddColumn("dbo.User", "Group_GroupId1", c => c.Int());
            CreateIndex("dbo.User", "Group_GroupId1");
            AddForeignKey("dbo.User", "Group_GroupId", "dbo.Group", "GroupId");
            AddForeignKey("dbo.User", "Group_GroupId1", "dbo.Group", "GroupId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "Group_GroupId1", "dbo.Group");
            DropForeignKey("dbo.User", "Group_GroupId", "dbo.Group");
            DropIndex("dbo.User", new[] { "Group_GroupId1" });
            DropColumn("dbo.User", "Group_GroupId1");
            AddForeignKey("dbo.User", "Group_GroupId", "dbo.Group", "GroupId");
        }
    }
}
