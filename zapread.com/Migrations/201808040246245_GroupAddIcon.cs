namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupAddIcon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Group", "Icon", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Group", "Icon");
        }
    }
}
