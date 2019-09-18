namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class paymentLimboFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LNTransaction", "IsLimbo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LNTransaction", "IsLimbo");
        }
    }
}
