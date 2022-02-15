namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class expandLNTransactionFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "PreimageHash", c => c.String());
            AddColumn("dbo.LNTransaction", "PaymentHash", c => c.String());
            AddColumn("dbo.LNTransaction", "TimestampUpdated", c => c.DateTime());
            AddColumn("dbo.LNTransaction", "PaymentIndex", c => c.Int());
            AddColumn("dbo.LNTransaction", "AddIndex", c => c.Int());
            AddColumn("dbo.LNTransaction", "SettleIndex", c => c.Int());
            AddColumn("dbo.LNTransaction", "FailureReason", c => c.String());
            AddColumn("dbo.LNTransaction", "PaymentStatus", c => c.String());
            AddColumn("dbo.LNTransaction", "InvoiceState", c => c.String());
            AddColumn("dbo.LNTransaction", "IsKeysend", c => c.Boolean());
            AddColumn("dbo.LNTransaction", "PaymentPreimage", c => c.String());
            AddColumn("dbo.LNTransaction", "ZapreadNode_Id", c => c.Int());
            CreateIndex("dbo.LNTransaction", "ZapreadNode_Id");
            AddForeignKey("dbo.LNTransaction", "ZapreadNode_Id", "dbo.LNNode", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LNTransaction", "ZapreadNode_Id", "dbo.LNNode");
            DropIndex("dbo.LNTransaction", new[] { "ZapreadNode_Id" });
            DropColumn("dbo.LNTransaction", "ZapreadNode_Id");
            DropColumn("dbo.LNTransaction", "PaymentPreimage");
            DropColumn("dbo.LNTransaction", "IsKeysend");
            DropColumn("dbo.LNTransaction", "InvoiceState");
            DropColumn("dbo.LNTransaction", "PaymentStatus");
            DropColumn("dbo.LNTransaction", "FailureReason");
            DropColumn("dbo.LNTransaction", "SettleIndex");
            DropColumn("dbo.LNTransaction", "AddIndex");
            DropColumn("dbo.LNTransaction", "PaymentIndex");
            DropColumn("dbo.LNTransaction", "TimestampUpdated");
            DropColumn("dbo.LNTransaction", "PaymentHash");
            DropColumn("dbo.LNTransaction", "PreimageHash");
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
