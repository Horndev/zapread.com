namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLNTransaction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LNTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentRequest = c.String(),
                        HashStr = c.String(),
                        TimestampSettled = c.DateTime(),
                        TimestampCreated = c.DateTime(),
                        Amount = c.Long(nullable: false),
                        Memo = c.String(),
                        IsDeposit = c.Boolean(nullable: false),
                        IsSettled = c.Boolean(nullable: false),
                        FeePaid_Satoshi = c.Long(),
                        NodePubKey = c.String(),
                        ErrorMessage = c.String(),
                        IsError = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LNTransaction", "User_Id", "dbo.User");
            DropIndex("dbo.LNTransaction", new[] { "User_Id" });
            DropTable("dbo.LNTransaction");
        }
    }
}
