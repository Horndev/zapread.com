using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Database.Financial;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountMonitorService
    {
        /// <summary>
        /// 
        /// </summary>
        public void CheckAccountLocks()
        {
            using (var db = new ZapContext())
            {
                var locks = db.Locks.ToList();

                List<FundsLock> locksToRemove = new List<FundsLock>();
                var now = DateTime.UtcNow;
                foreach (var l in locks)
                {
                    if (l.TimeStampExpired < now)
                    {
                        locksToRemove.Add(l);
                    }
                }

                foreach (var x in locksToRemove)
                {
                    db.Locks.Remove(x);
                }

                db.SaveChanges();
            }
        }
    }
}