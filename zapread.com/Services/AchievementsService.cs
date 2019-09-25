using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Database;

namespace zapread.com.Services
{
    public class AchievementsService
    {
        public static List<IAchievementCriteria> Achievements = new List<IAchievementCriteria>()
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
                    foreach(var ukvp in uas)
                    {
                        ukvp.Key.Achievements.Add(ukvp.Value);
                    }
                    
                    db.SaveChanges();
                }
            }
        }
    }

    public class ThousandImpressions : IAchievementCriteria
    {
        public string Name { get => "1,000 Impressions"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 1000).Any());

            return newUsersAchieved;
        }
    }
    public class FiveHunderedImpressions : IAchievementCriteria
    {
        public string Name { get => "500 Impressions"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 500).Any());

            return newUsersAchieved;
        }
    }
    public class HunderedImpressions : IAchievementCriteria
    {
        public string Name { get => "100 Impressions"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Where(p => p.Impressions >= 100).Any());

            return newUsersAchieved;
        }
    }
    public class FirstLNWithdraw : IAchievementCriteria
    {
        public string Name { get => "First LN Withdraw"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.LNTransactions.Where(t => !t.IsDeposit && t.IsSettled).Count() > 0);

            return newUsersAchieved;
        }
    }
    public class FirstLNDeposit : IAchievementCriteria
    {
        public string Name { get => "First LN Deposit"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.LNTransactions.Where(t => t.IsDeposit && t.IsSettled).Count() > 0);

            return newUsersAchieved;
        }
    }
    public class HunderedThousandReputation : IAchievementCriteria
    {
        public string Name { get => "100,000 Reputation"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 100000);

            return newUsersAchieved;
        }
    }
    public class TenThousandReputation : IAchievementCriteria
    {
        public string Name { get => "10,000 Reputation"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 10000);

            return newUsersAchieved;
        }
    }
    public class OneThousandReputation : IAchievementCriteria
    {
        public string Name { get => "1000 Reputation"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Reputation >= 1000);

            return newUsersAchieved;
        }
    }
    public class FirstComment : IAchievementCriteria
    {
        public string Name { get => "First Comment"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Comments.Count() > 0);

            return newUsersAchieved;
        }
    }
    public class FirstFollowed : IAchievementCriteria
    {
        public string Name { get => "First Followed"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Followers.Count() > 0);

            return newUsersAchieved;
        }
    }
    public class FirstFollowing : IAchievementCriteria
    {
        public string Name { get => "First Following"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Following.Count() > 0);

            return newUsersAchieved;
        }
    }
    public class FirstPost : IAchievementCriteria
    {
        public string Name { get => "First Post"; }

        public IQueryable<User> GetNewUsers(ZapContext db, Achievement dba)
        {
            // Check who has the criteria
            var newUsersAchieved = db.Users
                .Where(u => !u.Achievements.Select(ua => ua.Achievement.Id).Contains(dba.Id))
                .Where(u => u.Posts.Count() > 0);

            return newUsersAchieved;
        }
    }
    public interface IAchievementCriteria
    {
        string Name { get; }

        IQueryable<User> GetNewUsers(ZapContext db, Achievement dba);

    }
}