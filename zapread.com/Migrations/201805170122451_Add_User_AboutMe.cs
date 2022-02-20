namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// 
    /// </summary>
    public partial class Add_User_AboutMe : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.User", "AboutMe", c => c.String());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.User", "AboutMe");
        }
    }
}
