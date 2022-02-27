namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class AddEarningsCommunityWebsite : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ZapReadGlobals",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ZapReadEarnedBalance = c.Double(nullable: false),
                    ZapReadTotalEarned = c.Double(nullable: false),
                    ZapReadTotalWithdrawn = c.Double(nullable: false),
                    CommunityEarnedToDistribute = c.Double(nullable: false),
                    TotalEarnedCommunity = c.Double(nullable: false),
                    TotalDepositedCommunity = c.Double(nullable: false),
                    TotalWithdrawnCommunity = c.Double(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.EarningEvent", "Group_GroupId", c => c.Int());
            AddColumn("dbo.Group", "TotalEarnedToDistribute", c => c.Double(nullable: false));
            AddColumn("dbo.Group", "TotalEarned", c => c.Double(nullable: false));
            AddColumn("dbo.LNTransaction", "ZapReadGlobals_Id", c => c.Int());
            CreateIndex("dbo.EarningEvent", "Group_GroupId");
            CreateIndex("dbo.LNTransaction", "ZapReadGlobals_Id");
            AddForeignKey("dbo.EarningEvent", "Group_GroupId", "dbo.Group", "GroupId");
            AddForeignKey("dbo.LNTransaction", "ZapReadGlobals_Id", "dbo.ZapReadGlobals", "Id");
            DropColumn("dbo.Group", "Earned");
        }

        public override void Down()
        {
            AddColumn("dbo.Group", "Earned", c => c.Double(nullable: false));
            DropForeignKey("dbo.LNTransaction", "ZapReadGlobals_Id", "dbo.ZapReadGlobals");
            DropForeignKey("dbo.EarningEvent", "Group_GroupId", "dbo.Group");
            DropIndex("dbo.LNTransaction", new[] { "ZapReadGlobals_Id" });
            DropIndex("dbo.EarningEvent", new[] { "Group_GroupId" });
            DropColumn("dbo.LNTransaction", "ZapReadGlobals_Id");
            DropColumn("dbo.Group", "TotalEarned");
            DropColumn("dbo.Group", "TotalEarnedToDistribute");
            DropColumn("dbo.EarningEvent", "Group_GroupId");
            DropTable("dbo.ZapReadGlobals");
        }
    }
}
