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
            new FirstPost()
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