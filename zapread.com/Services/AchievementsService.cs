using System;
using System.Collections.Generic;
using System.Linq;
using zapread.com.Database;
using zapread.com.Models.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// Service for managing user achievements
    /// </summary>
    public class AchievementsService
    {
        /// <summary>
        /// List of the available achievements
        /// </summary>
        public static readonly List<IAchievementCriteria> Achievements = new List<IAchievementCriteria>()
        {
            new FirstPost(),
            new FirstFollowing(),
            new FirstFollowed(),
            new FirstComment(),
            new OneThousandReputation(),
            new FirstLNDeposit(),
            new FirstLNWithdraw(),
            new TenThousandReputation(),
            new HunderedThousandReputation(),
            new HunderedImpressions(),
            new FiveHunderedImpressions(),
            new ThousandImpressions(),
        };

        /// <summary>
        /// Check the database for any new achievements and award them
        /// </summary>
        public void CheckAchievements()
        {
            using (var db = new ZapContext())
            {
                // Check each achievement
                foreach (var a in Achievements)
                {
                    var dba = db.Achievements
                        .FirstOrDefault(i => i.Name == a.Name);

                    if (dba == null)
                    {
                        continue;
                    }

                    var newUsers = a.GetNewUsers(db, dba);

                    var c = newUsers.Count();

                    // We need to use a dictionary to save the results before applying to db since the next
                    // foreach will be an open DB connection (can't query and apply at same time).
                    Dictionary<User, UserAchievement> uas = new Dictionary<User, UserAchievement>();
                    foreach (var u in newUsers)
                    {
                        var usr = u.Name;
                        var ua = new UserAchievement()
                        {
                            AchievedBy = u,
                            Achievement = dba,
                            DateAchieved = DateTime.UtcNow,
                        };
                        uas.Add(u, ua);
                    }

                    // Apply db updates to users
                    foreach (var ukvp in uas)
                    {
                        ukvp.Key.Achievements.Add(ukvp.Value);
                    }

                    db.SaveChanges();
                }
            }
        }
    }

    /// <summary>
    /// Post received 1000 impressions
    /// </summary>
    public class ThousandImpressions : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "1,000 Impressions"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 1000).Any());

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FiveHunderedImpressions : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "500 Impressions"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 500).Any());

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HunderedImpressions : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "100 Impressions"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 100).Any());

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstLNWithdraw : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First LN Withdraw"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.LNTransactions.Where(t => !t.IsDeposit && t.IsSettled).Count() > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstLNDeposit : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First LN Deposit"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.LNTransactions.Where(t => t.IsDeposit && t.IsSettled).Count() > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HunderedThousandReputation : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "100,000 Reputation"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 100000);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TenThousandReputation : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "10,000 Reputation"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 10000);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OneThousandReputation : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "1000 Reputation"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 1000);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstComment : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First Comment"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Comments.Count > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstFollowed : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First Followed"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Followers.Count > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstFollowing : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First Following"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Following.Count > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FirstPost : IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        public string Name { get => "First Post"; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Count > 0);

            return newUsersAchieved;
        }
    }

    /// <summary>
    /// Describes an achievement
    /// </summary>
    public interface IAchievementCriteria
    {
        /// <summary>
        /// Name of the achievement
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Method which provides the query parameters for users which should receive this achievement
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dba"></param>
        /// <returns></returns>
        IQueryable<User> GetNewUsers(ZapContext db, Achievement dba);

    }
}