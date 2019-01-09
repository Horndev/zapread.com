namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLNTransactionIgnore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "IsIgnored", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "IsIgnored");
        }
    }
}
