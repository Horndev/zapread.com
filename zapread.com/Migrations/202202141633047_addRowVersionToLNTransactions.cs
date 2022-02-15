namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRowVersionToLNTransactions : DbMigration
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "RowVersion");
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
