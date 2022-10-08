using Hangfire;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.API;
using zapread.com.Models.API.Stream;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// API interface for websocket real-time streaming
    /// </summary>
    public class StreamController : ApiController
    {
        /// <summary>
        /// Added default route for request since optional parameter was not working
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/stream/request")]
        public ConnectionInfoResponse RequestDefaultConnection()
        {
            return RequestConnection(null);
        }

        /// <summary>
        /// Request connection information for a streaming socket.
        /// </summary>
        /// <param name = "ctoken">connection token to use for stream subscription when not authenticated</param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/stream/request/{ctoken?}")]
        public ConnectionInfoResponse RequestConnection(string ctoken)
        {
            var url = ConfigurationManager.AppSettings.Get("wshost");
            if ((ctoken == null) && base.User.Identity.IsAuthenticated)
            {
                var token = User.Identity.GetUserId();
                url = url + "/notificationHub?a=" + token;
            }
            else if (ctoken != null)
            {
                url = url + "/notificationHub?a=" + ctoken;
            }
            else
            {
                url += "/notificationHub";
            }

            return new ConnectionInfoResponse()
            {success = true, url = url};
        }

        /// <summary>
        /// Notify a user has connected
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        //[Authorize]  // Need to set up a key for these requests.  No harm done but it makes it tighter.
        [Route("api/v1/stream/notify/connected/{userAppId}")]
        public async Task<ZapReadResponse> NotifyConnected(string userAppId)
        {
            try
            {
                using (var db = new ZapContext())
                {
                    var user = await db.Users.SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);
                    if (user == null)
                    {
                        return new ZapReadResponse()
                        {success = false, };
                    }

                    user.IsOnline = true;
                    user.DateLastActivity = DateTime.UtcNow;
                    // Performance - this gets called often, so we don't need to transfer all info, just the jobid
                    //var jobq = await db.Users
                    //    .Where(u => u.AppId == userAppId)
                    //    .Select(u => new
                    //    {
                    //        u.PGPPubKey, // Hack: this is actually the job ID.
                    //            u.Name,
                    //    })
                    //    .SingleOrDefaultAsync().ConfigureAwait(false);
                    if (String.IsNullOrEmpty(user.PGPPubKey))
                    {
                        // If LastActivity is not updated in the last 10 minutes, then user will go offline.
                        var jobId = BackgroundJob.Schedule<UserState>(methodCall: x => x.UserOffline(userAppId, user.Name, DateTime.UtcNow), delay: TimeSpan.FromMinutes(10));
                        //var user = await db.Users
                        //    .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);
                        // Save the jobId so we don't schedule another check
                        user.PGPPubKey = jobId;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
            }
            catch
            {
            }

            return new ZapReadResponse()
            {success = true, };
        }

        /// <summary>
        /// Notify a user has disconnected
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Authorize]
        [Route("api/v1/stream/notify/disconnected/{userAppId}")]
        public async Task<ZapReadResponse> NotifyDisconnected(string userAppId)
        {
            try
            {
                using (var db = new ZapContext())
                {
                    // Performance - this gets called often, so we don't need to transfer all info, just the jobid
                    var jobq = await db.Users.Where(u => u.AppId == userAppId).Select(u => new
                    {
                    u.PGPPubKey, // Hack: this is actually the job ID.
 u.Name, }).SingleOrDefaultAsync().ConfigureAwait(false);
                    if (String.IsNullOrEmpty(jobq.PGPPubKey))
                    {
                        // If LastActivity is not updated in the last 10 minutes, then user will go offline.
                        var jobId = BackgroundJob.Schedule<UserState>(methodCall: x => x.UserOffline(userAppId, jobq.Name, DateTime.UtcNow), delay: TimeSpan.FromMinutes(10));
                        var user = await db.Users.SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);
                        // Save the jobId so we don't schedule another check
                        user.PGPPubKey = jobId;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
            }
            catch
            {
            }

            return new ZapReadResponse()
            {success = true, };
        }
    }
}