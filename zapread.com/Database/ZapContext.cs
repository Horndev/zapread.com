using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text.RegularExpressions;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Database.Financial;
using zapread.com.Models.Lightning;

namespace zapread.com.Database
{
    /// <summary>
    /// Main Database Context
    /// </summary>
    public class ZapContext : DbContext
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ZapContext() : base("name=" + System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"])
        {
            //DbInterception.Add(FreetextInterceptor.Instance);
        }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Models.Database.Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserImage> Images { get; set; }
        public DbSet<LNTransaction> LightningTransactions { get; set; }
        public DbSet<EarningEvent> EarningEvents { get; set; }
        public DbSet<SpendingEvent> SpendingEvents { get; set; }
        public DbSet<ZapReadGlobals> ZapreadGlobals { get; set; }
        public DbSet<ZapIcon> Icons { get; set; }
        public DbSet<UserMessage> Messages { get; set; }
        public DbSet<UserAlert> Alerts { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<PendingPostVote> PendingPostVotes { get; set; }
        public DbSet<PendingCommentVote> PendingCommentVotes { get; set; }
        public DbSet<HourlyStatistics> HourlyStatistics { get; set; }
        public DbSet<DailyStatistics> DailyStatistics { get; set; }
        public DbSet<WeeklyStatistics> WeeklyStatistics { get; set; }
        public DbSet<MonthlyStatistics> MonthlyStatistics { get; set; }
        public DbSet<LNNode> LNNodes { get; set; }
        public DbSet<APIKey> APIKeys { get; set; }

        /// <summary>
        /// Reactions used for posts and comments
        /// </summary>
        public DbSet<Reaction> Reactions { get; set; }

        /// <summary>
        /// Existing and known tags
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FundsLock> Locks { get; set; }

        /// <summary>
        /// List of user Balances
        /// </summary>
        public DbSet<UserFunds> Funds { get; set; }

        /// <summary>
        /// Alerts to show to users as a Banner on main page
        /// </summary>
        public DbSet<BannerAlert> BannerAlerts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}