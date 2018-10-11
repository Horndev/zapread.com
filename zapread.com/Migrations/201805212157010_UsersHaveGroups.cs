namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UsersHaveGroups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "User_Id", c => c.Int());
            CreateIndex("dbo.Group", "User_Id");
            AddForeignKey("dbo.Group", "User_Id", "dbo.User", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Group", "User_Id", "dbo.User");
            DropIndex("dbo.Group", new[] { "User_Id" });
            DropColumn("dbo.Group", "User_Id");
        }
    }
}
