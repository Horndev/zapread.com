namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentTimestampEdited : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comment", "TimeStampEdited", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comment", "TimeStampEdited");
        }
    }
}
