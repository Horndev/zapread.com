namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupAdminFKs : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.GroupUser", newName: "GroupUser1");
            DropForeignKey("dbo.User", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.User", "Group_GroupId1", "dbo.Group");
            DropIndex("dbo.User", new[] { "Group_GroupId" });
            DropIndex("dbo.User", new[] { "Group_GroupId1" });
            CreateTable(
                "dbo.GroupUser",
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
            
            CreateTable(
                "dbo.GroupUser2",
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
            
            DropColumn("dbo.User", "Group_GroupId");
            DropColumn("dbo.User", "Group_GroupId1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "Group_GroupId1", c => c.Int());
            AddColumn("dbo.User", "Group_GroupId", c => c.Int());
            DropForeignKey("dbo.GroupUser2", "User_Id", "dbo.User");
            DropForeignKey("dbo.GroupUser2", "Group_GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupUser", "User_Id", "dbo.User");
            DropForeignKey("dbo.GroupUser", "Group_GroupId", "dbo.Group");
            DropIndex("dbo.GroupUser2", new[] { "User_Id" });
            DropIndex("dbo.GroupUser2", new[] { "Group_GroupId" });
            DropIndex("dbo.GroupUser", new[] { "User_Id" });
            DropIndex("dbo.GroupUser", new[] { "Group_GroupId" });
            DropTable("dbo.GroupUser2");
            DropTable("dbo.GroupUser");
            CreateIndex("dbo.User", "Group_GroupId1");
            CreateIndex("dbo.User", "Group_GroupId");
            AddForeignKey("dbo.User", "Group_GroupId1", "dbo.Group", "GroupId");
            AddForeignKey("dbo.User", "Group_GroupId", "dbo.Group", "GroupId");
            RenameTable(name: "dbo.GroupUser1", newName: "GroupUser");
        }
    }
}
