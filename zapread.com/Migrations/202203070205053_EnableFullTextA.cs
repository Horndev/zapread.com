namespace zapread.com.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// Create a fulltext index
    /// 
    /// 
    /// </summary>
    public partial class EnableFullTextA : DbMigration
    {
        /// <summary>
        /// 
        /// </summary>
        public override void Up()
        {
            // This is our catalog for searching
            Sql("CREATE FULLTEXT CATALOG zapread_catalog AS DEFAULT;", suppressTransaction: true);
            Sql("CREATE FULLTEXT INDEX ON [dbo].[Comment] ([Text]) "+
                    "KEY INDEX[PK_dbo.Comment] ON zapread_catalog "+
                    "WITH STOPLIST = SYSTEM, CHANGE_TRACKING = AUTO;", suppressTransaction: true);
            Sql("CREATE FULLTEXT INDEX ON [dbo].[Post] ([Content]) " +
                    "KEY INDEX[PK_dbo.Post] ON zapread_catalog " +
                    "WITH STOPLIST = SYSTEM, CHANGE_TRACKING = AUTO;", suppressTransaction: true);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            Sql("DROP FULLTEXT INDEX ON [dbo].[Post];", suppressTransaction: true);
            Sql("DROP FULLTEXT INDEX ON [dbo].[Comment];", suppressTransaction: true);
            Sql("DROP FULLTEXT CATALOG zapread_catalog;", suppressTransaction: true);
        }
    }
}
