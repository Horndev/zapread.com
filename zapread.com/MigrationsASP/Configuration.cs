namespace zapread.com.MigrationsASP
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    /// <summary>
    /// Add-Migration -configuration zapread.com.MigrationsASP.Configuration Initial
    /// 
    /// Add-Migration -configuration zapread.com.MigrationsASP.Configuration AddUserEmailAuthOption
    /// 
    /// 
    /// Update-Database -configuration zapread.com.MigrationsASP.Configuration -Verbose
    /// 
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<zapread.com.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"MigrationsASP";
            ContextKey = "zapread.com.Models.ApplicationDbContext";
        }

        protected override void Seed(zapread.com.Models.ApplicationDbContext context)
        {
        //  This method will be called after migrating to the latest version.
        //  You can use the DbSet<T>.AddOrUpdate() helper extension method
        //  to avoid creating duplicate seed data.
        }
    }
}