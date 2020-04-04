using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using zapread.com.Database;
using zapread.com.Services;

namespace zapread.com.Hubs
{
    public class NotificationHub : Hub
    {
        // Send notification to all users
        public void NotifyInvoicePaid(string invoice)
        {
            Clients.All.notifyInvoicePaid(invoice);
        }

        // Send notification only to user
        public void NotifyInvoicePaid(string invoice, string userId)
        {
            Clients.Group(userId).notifyInvoicePaid(invoice);
        }

        public void SendUserMessage(string message, string userId)
        {
            Clients.Group(userId).sendUserMessage(message);
        }

        public override async Task OnConnected()
        {
            string userAppId = Context.User.Identity.GetUserId();

            if (userAppId != null)
            {
                await Groups.Add(Context.ConnectionId, userAppId).ConfigureAwait(true);

                try
                {
                    using (var db = new ZapContext())
                    {
                        // idea for this from https://stackoverflow.com/questions/4218566/update-a-record-without-first-querying
                        // we want this to be fast - so we reduce the trips to DB

                        // This does seem to work - could introduce in future for performance improvement
                        //User u = db.Users.Attach(new User { AppId = userAppId });
                        //u.IsOnline = true;
                        //db.Entry<User>(u).Property(ee => ee.IsOnline).IsModified = true;
                        //db.Configuration.ValidateOnSaveEnabled = false;
                        //await db.SaveChangesAsync().ConfigureAwait(false);

                        var user = await db.Users
                            .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);

                        user.IsOnline = true;
                        user.DateLastActivity = DateTime.UtcNow;
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
                catch
                {

                }
            }
            await base.OnConnected().ConfigureAwait(true);
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            string userAppId = Context.User.Identity.GetUserId();

            if (userAppId != null)
            {
                try
                {
                    using (var db = new ZapContext())
                    {
                        // Performance - this gets called often, so we don't need to transfer all info, just the jobid
                        var jobq = await db.Users
                            .Where(u => u.AppId == userAppId)
                            .Select(u => new
                            {
                                u.PGPPubKey, // Hack: this is actually the job ID.
                                u.Name,
                            })
                            .SingleOrDefaultAsync().ConfigureAwait(false);

                        if (String.IsNullOrEmpty(jobq.PGPPubKey))
                        {
                            // If LastActivity is not updated in the last 10 minutes, then user will go offline.
                            var jobId = BackgroundJob.Schedule<UserState>(
                                methodCall: x => x.UserOffline(userAppId, jobq.Name, DateTime.UtcNow),
                                delay: TimeSpan.FromMinutes(10));

                            var user = await db.Users
                                .SingleOrDefaultAsync(u => u.AppId == userAppId).ConfigureAwait(false);

                            // Save the jobId so we don't schedule another check
                            user.PGPPubKey = jobId;

                            await db.SaveChangesAsync().ConfigureAwait(true);
                        }
                    }
                }
                catch
                {

                }
            }
            await base.OnDisconnected(stopCalled).ConfigureAwait(true);
        }
    }
}