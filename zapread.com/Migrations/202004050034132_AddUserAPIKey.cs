namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddUserAPIKey : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.APIKey",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        Roles = c.String(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.APIKey", "User_Id", "dbo.User");
            DropIndex("dbo.APIKey", new[] { "User_Id" });
            DropTable("dbo.APIKey");
        }
    }
}
