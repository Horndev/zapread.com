namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentInvoiceInfo : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.SubscriptionPayment", "InvoiceId", c => c.String());
            AddColumn("dbo.SubscriptionPayment", "ReceiptUrl", c => c.String());
            AddColumn("dbo.SubscriptionPayment", "IsPaid", c => c.Boolean(nullable: false));
            AddColumn("dbo.SubscriptionPayment", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.SubscriptionPayment", "RowVersion");
            DropColumn("dbo.SubscriptionPayment", "IsPaid");
            DropColumn("dbo.SubscriptionPayment", "ReceiptUrl");
            DropColumn("dbo.SubscriptionPayment", "InvoiceId");
        }
    }
}
