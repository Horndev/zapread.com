namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member    
    public partial class AddReferralTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Referral",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReferredByAppId = c.String(),
                        TimeStamp = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.User", "ReferralCode", c => c.String());
            AddColumn("dbo.User", "ReferralInfo_Id", c => c.Int());
            CreateIndex("dbo.User", "ReferralInfo_Id");
            AddForeignKey("dbo.User", "ReferralInfo_Id", "dbo.Referral", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "ReferralInfo_Id", "dbo.Referral");
            DropIndex("dbo.User", new[] { "ReferralInfo_Id" });
            DropColumn("dbo.User", "ReferralInfo_Id");
            DropColumn("dbo.User", "ReferralCode");
            DropTable("dbo.Referral");
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
