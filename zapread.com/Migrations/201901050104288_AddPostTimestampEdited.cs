namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostTimestampEdited : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "TimeStampEdited", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Post", "TimeStampEdited");
        }
    }
}
