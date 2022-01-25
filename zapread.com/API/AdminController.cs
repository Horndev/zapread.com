using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// Administrative API
    /// </summary>
    [Authorize]
    [Route("api/v1/admin")]
    public class AdminController : ApiController
    {
        /// <summary>
        /// Refresh check if user is online.  This is needed sometimes when the DB is out of sync.
        ///   Requires Administator role.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>success={true|false}</returns>
        [Route("api/v1/admin/checkonline/{userId}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator,APIUser")]
        public async Task<ZapReadResponse> CheckOnline(int userId)
        {
            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.Id == userId)
                    .SingleOrDefaultAsync().ConfigureAwait(false);

                var jobId = BackgroundJob.Schedule<UserState>(
                    methodCall: x => x.UserOffline(user.AppId, user.Name, DateTime.UtcNow),
                    delay: TimeSpan.FromMinutes(1));

                // Save the jobId so we don't schedule another check
                user.PGPPubKey = jobId;

                await db.SaveChangesAsync().ConfigureAwait(true);

                return new ZapReadResponse() { success = true };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Route("api/v1/admin/accounting/{year}/{month}")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> GetAccountingSummary(int year, int month)
        {
            using (var db = new ZapContext())
            {
                var startDate = DateTime.Parse("1 January 2022");
                
                //var transactions = db.
            }

            return Ok();
        }
    }
}
