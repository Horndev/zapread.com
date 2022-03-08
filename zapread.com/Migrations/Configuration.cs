namespace zapread.com.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Add-Migration -configuration zapread.com.Migrations.Configuration EnableFullTextA
    /// 
    /// 
    /// Update-Database -configuration zapread.com.Migrations.Configuration -Verbose 
    /// 
    /// Update-Database -configuration zapread.com.Migrations.Configuration -TargetMigration:"AddReferralTable" -Verbose 
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<zapread.com.Database.ZapContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations";
            ContextKey = "zapread.com.Migrations.Configuration"; //  zapread.com.Database.ZapContext
        }

        protected override void Seed(zapread.com.Database.ZapContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
