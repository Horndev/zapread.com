using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// Manages background jobs related to user status
    /// </summary>
    public class UserState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppid"></param>
        public void UserOnline(string userAppid)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .SingleOrDefault(u => u.AppId == userAppid);

                if (user != null)
                {
                    user.DateLastActivity = DateTime.UtcNow;
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// Checks if user is still online - if not, marks as offline.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public void UserOffline(string appId, string name, DateTime t)
        {
            using (var db = new ZapContext())
            {
                var user = db.Users
                    .SingleOrDefault(u => u.AppId == appId);

                var tElapsed = user.DateLastActivity - t;
                if (tElapsed < TimeSpan.FromMinutes(30))  // If the user has not loaded any pages in the last 30 minutes, mark as offline.
                {
                    user.PGPPubKey = "";
                    user.IsOnline = false;
                    db.SaveChanges();
                }
                else
                {
                    // re-enqueue to check later in another 10 minutes.
                    var jobId = BackgroundJob.Schedule<UserState>(
                                methodCall: x => x.UserOffline(appId, name, DateTime.UtcNow),
                                delay: TimeSpan.FromMinutes(10));

                    user.PGPPubKey = jobId;
                    db.SaveChanges();
                }
            }
        }
    }
}