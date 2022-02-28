namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddLNConfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ZapReadGlobals", "LnMainnetHost", c => c.String());
            AddColumn("dbo.ZapReadGlobals", "LnPubkey", c => c.String());
            AddColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonInvoice", c => c.String());
            AddColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonRead", c => c.String());
            AddColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonAdmin", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonAdmin");
            DropColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonRead");
            DropColumn("dbo.ZapReadGlobals", "LnMainnetMacaroonInvoice");
            DropColumn("dbo.ZapReadGlobals", "LnPubkey");
            DropColumn("dbo.ZapReadGlobals", "LnMainnetHost");
        }
    }
}
