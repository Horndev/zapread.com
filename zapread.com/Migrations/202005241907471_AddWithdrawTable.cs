namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWithdrawTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Withdraw",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Invoice = c.String(),
                        Amount = c.Double(nullable: false),
                        ValidationTimestamp = c.DateTime(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.UserFunds", "IsWithdrawLocked", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserFunds", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Withdraw", "User_Id", "dbo.User");
            DropIndex("dbo.Withdraw", new[] { "User_Id" });
            DropColumn("dbo.UserFunds", "RowVersion");
            DropColumn("dbo.UserFunds", "IsWithdrawLocked");
            DropTable("dbo.Withdraw");
        }
    }
}
