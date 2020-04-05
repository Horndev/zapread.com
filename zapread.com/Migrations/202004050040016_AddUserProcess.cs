namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserProcess : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProcess",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.String(),
                        JobName = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserProcess", "User_Id", "dbo.User");
            DropIndex("dbo.UserProcess", new[] { "User_Id" });
            DropTable("dbo.UserProcess");
        }
    }
}
