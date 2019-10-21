namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LNNodesAndStats : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DailyStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StatisticsPoint",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(),
                        NumNewUsers = c.Int(nullable: false),
                        NumNewPosts = c.Int(nullable: false),
                        NumNewComments = c.Int(nullable: false),
                        NumNewImages = c.Int(nullable: false),
                        NumPrivateMessages = c.Int(nullable: false),
                        Deposited_Satoshi = c.Int(nullable: false),
                        Withdrawn_Satoshi = c.Int(nullable: false),
                        NumVotes = c.Int(nullable: false),
                        NumTips = c.Int(nullable: false),
                        TotalTipped = c.Int(nullable: false),
                        DailyStatistics_Id = c.Int(),
                        HourlyStatistics_Id = c.Int(),
                        MonthlyStatistics_Id = c.Int(),
                        WeeklyStatistics_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DailyStatistics", t => t.DailyStatistics_Id)
                .ForeignKey("dbo.HourlyStatistics", t => t.HourlyStatistics_Id)
                .ForeignKey("dbo.MonthlyStatistics", t => t.MonthlyStatistics_Id)
                .ForeignKey("dbo.WeeklyStatistics", t => t.WeeklyStatistics_Id)
                .Index(t => t.DailyStatistics_Id)
                .Index(t => t.HourlyStatistics_Id)
                .Index(t => t.MonthlyStatistics_Id)
                .Index(t => t.WeeklyStatistics_Id);
            
            CreateTable(
                "dbo.HourlyStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LNNode",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Alias = c.String(),
                        Version = c.String(),
                        IsTestnet = c.Boolean(nullable: false),
                        PubKey = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LNChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPrivate = c.Boolean(nullable: false),
                        IsOnline = c.Boolean(nullable: false),
                        IsLocalInitiator = c.Boolean(nullable: false),
                        RemotePubKey = c.String(),
                        RemoteAlias = c.String(),
                        ChannelPoint = c.String(),
                        ChannelId = c.String(),
                        TotalSent_MilliSatoshi = c.Long(nullable: false),
                        TotalReceived_MilliSatoshi = c.Long(nullable: false),
                        Capacity_MilliSatoshi = c.Long(nullable: false),
                        LocalReserve_MilliSatoshi = c.Long(nullable: false),
                        RemoteReserve_MilliSatoshi = c.Long(nullable: false),
                        LNNode_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LNNode", t => t.LNNode_Id)
                .Index(t => t.LNNode_Id);
            
            CreateTable(
                "dbo.LNChannelHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(),
                        IsOnline = c.Boolean(nullable: false),
                        LocalBalance_MilliSatoshi = c.Long(nullable: false),
                        RemoteBalance_MilliSatoshi = c.Long(nullable: false),
                        Channel_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LNChannel", t => t.Channel_Id)
                .Index(t => t.Channel_Id);
            
            CreateTable(
                "dbo.LNNodeVersionHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(),
                        Version = c.String(),
                        Node_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LNNode", t => t.Node_Id)
                .Index(t => t.Node_Id);
            
            CreateTable(
                "dbo.MonthlyStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.WeeklyStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StatisticsPoint", "WeeklyStatistics_Id", "dbo.WeeklyStatistics");
            DropForeignKey("dbo.StatisticsPoint", "MonthlyStatistics_Id", "dbo.MonthlyStatistics");
            DropForeignKey("dbo.LNNodeVersionHistory", "Node_Id", "dbo.LNNode");
            DropForeignKey("dbo.LNChannel", "LNNode_Id", "dbo.LNNode");
            DropForeignKey("dbo.LNChannelHistory", "Channel_Id", "dbo.LNChannel");
            DropForeignKey("dbo.StatisticsPoint", "HourlyStatistics_Id", "dbo.HourlyStatistics");
            DropForeignKey("dbo.StatisticsPoint", "DailyStatistics_Id", "dbo.DailyStatistics");
            DropIndex("dbo.LNNodeVersionHistory", new[] { "Node_Id" });
            DropIndex("dbo.LNChannelHistory", new[] { "Channel_Id" });
            DropIndex("dbo.LNChannel", new[] { "LNNode_Id" });
            DropIndex("dbo.StatisticsPoint", new[] { "WeeklyStatistics_Id" });
            DropIndex("dbo.StatisticsPoint", new[] { "MonthlyStatistics_Id" });
            DropIndex("dbo.StatisticsPoint", new[] { "HourlyStatistics_Id" });
            DropIndex("dbo.StatisticsPoint", new[] { "DailyStatistics_Id" });
            DropTable("dbo.WeeklyStatistics");
            DropTable("dbo.MonthlyStatistics");
            DropTable("dbo.LNNodeVersionHistory");
            DropTable("dbo.LNChannelHistory");
            DropTable("dbo.LNChannel");
            DropTable("dbo.LNNode");
            DropTable("dbo.HourlyStatistics");
            DropTable("dbo.StatisticsPoint");
            DropTable("dbo.DailyStatistics");
        }
    }
}
