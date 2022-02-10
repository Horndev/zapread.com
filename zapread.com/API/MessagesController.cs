using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Models.Database;

namespace zapread.com.API
{
    /// <summary>
    /// WebAPI controller for messages
    /// </summary>
    public class MessagesController : ApiController
    {
        /// <summary>
        /// Get the list of user chats
        /// </summary>
        /// <param name="page">0..n</param>
        /// <param name="pagesize">default 20</param>
        /// <param name="sort">unread, recent (default)</param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/chats/list/{page}/{pagesize?}/{sort?}")]
        public async Task<IHttpActionResult> GetChats(int? page, int pagesize = 100, string sort = "recent")
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                string userAppId = user.AppId;

                // Get private messages
                var chatsQ = db.Messages
                    .Where(m => m.IsPrivateMessage == true)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.To.AppId == userAppId || m.From.AppId == userAppId)
                    .Where(m => !(m.To.AppId == userAppId && m.From.AppId == userAppId)) // Can't chat with self
                    .Select(m => new
                    {
                        m.Id,
                        other = m.To.AppId == userAppId ? m.From : m.To,
                        otherId = m.To.AppId == userAppId ? m.From.AppId : m.To.AppId,
                        FromOnline = m.To.AppId == userAppId ? m.From.IsOnline : m.To.IsOnline,
                        IsRead = m.To.AppId == userAppId ? m.IsRead : true, // If we responded, it's read
                        IsReplied = m.From.AppId == userAppId,
                        m.TimeStamp,
                    })
                    .GroupBy(m => m.other)  // Group by person
                    .Select(x => x.OrderByDescending(y => y.TimeStamp).FirstOrDefault()); // Most recent

                if (sort == "unread")
                {
                    chatsQ = chatsQ
                        .OrderBy(q => q.IsRead);
                } else if (sort == "recent")
                {
                    chatsQ = chatsQ
                        .OrderByDescending(q => q.TimeStamp);
                }

                int numrec = await chatsQ.CountAsync().ConfigureAwait(true);

                int startpage = page ?? 0;

                var valuesQ = await chatsQ
                    .Skip(startpage*pagesize)
                    .Take(pagesize)
                    .Select(u => new
                    {
                        u.Id,
                        FromName = u.other.Name,
                        FromAppId = u.other.AppId,
                        FromProfileImageVersion = u.other.ProfileImage.Version,
                        u.FromOnline,
                        u.IsRead,
                        u.IsReplied,
                        u.TimeStamp,
                    })
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                return Ok(new { chats = valuesQ, numChats = numrec });
            }
        }

        /// <summary>
        /// Get the alerts for a user
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/alerts/get/{page}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> GetAlerts(int? page)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                string userAppId = user.AppId;

                int pagesize = 100;
                int qpage = page ?? 0;

                var alerts = await db.Alerts.Where(a => a.To.AppId == userAppId)
                    .Where(a => !a.IsRead && !a.IsDeleted)
                    .OrderByDescending(a => a.TimeStamp)
                    .Select(a => new
                    {
                        a.TimeStamp,
                        a.Id,
                        a.IsDeleted,
                        a.IsRead,
                        a.Title,
                        a.Content,
                        PostId = a.PostLink == null ? -1 : a.PostLink.PostId,
                        PostTitle = a.PostLink == null ? "" : a.PostLink.PostTitle,
                    }).Skip(qpage * pagesize).Take(pagesize)
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                // Query message count
                var numAlerts = await db.Alerts
                    .Where(a => a.To.AppId == userAppId)
                    .Where(a => !a.IsRead && !a.IsDeleted)
                    .CountAsync().ConfigureAwait(true);

                return Ok(new { alerts, numAlerts });
            }
        }

        /// <summary>
        /// Get the messages for a user
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/messages/get/{page}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> GetMessages(int? page)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                string userAppId = user.AppId;

                int pagesize = 100;
                int qpage = page ?? 0;

                // Query messages to show
                var messages = await db.Messages
                    .Where(m => m.To.AppId == userAppId)
                    .Where(m => !m.IsRead && !m.IsDeleted)
                    .OrderByDescending(m => m.TimeStamp)
                    .Select(m => new {
                        m.Id,
                        m.IsPrivateMessage,
                        m.Title,
                        m.Content,
                        m.TimeStamp,
                        FromName = m.From.Name,
                        PostTitle = m.PostLink == null ? "" : m.PostLink.PostTitle,
                        PostId = m.PostLink == null ? -1 : m.PostLink.PostId,
                        CommentId = m.CommentLink == null ? -1 : m.CommentLink.CommentId,
                    })
                    .Skip(qpage*pagesize).Take(pagesize)
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                // Query message count
                var numMessages = await db.Messages
                    .Where(m => m.To.AppId == userAppId)
                    .Where(m => !m.IsRead && !m.IsDeleted)
                    .CountAsync().ConfigureAwait(true);

                return Ok(new { messages, numMessages });
            }
        }

        /// <summary>
        /// Mark a user message as read
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/v1/messages/user/mark-read/{id}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> DismissUserMessage(int? id)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                if (id == -1)
                {
                    foreach (var m in db.Messages.Where(m => m.To.AppId == user.AppId))
                    {
                        m.IsRead = true;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);

                    return Ok(new { success = true });
                }

                var message = await db.Messages.Where(m => m.To.AppId == user.AppId).Where(m => m.Id == id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (message == null)
                {
                    return NotFound();
                }

                message.IsRead = true;
                await db.SaveChangesAsync().ConfigureAwait(true);

                return Ok(new { success = true });
            }
        }

        /// <summary>
        /// Delete a message sent to the user (authorized via API key or header)
        /// </summary>
        /// <param name="id">message id</param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/v1/messages/user/{id}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> DeleteUserMessage(int? id)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                if (id == -1)
                {
                    foreach(var m in db.Messages.Where(m => m.To.AppId == user.AppId))
                    {
                        m.IsDeleted = true;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);

                    return Ok(new { success = true });
                }
                
                var message = await db.Messages.Where(m => m.To.AppId == user.AppId).Where(m => m.Id == id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (message == null)
                {
                    return NotFound();
                }

                message.IsDeleted = true;
                await db.SaveChangesAsync().ConfigureAwait(true);

                return Ok(new { success = true });
            }
        }

        /// <summary>
        /// Mark a user alert as read
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [Route("api/v1/alerts/user/mark-read/{id}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> DismissUserAlert(int? id)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                if (id == -1)
                {
                    foreach (var m in db.Alerts.Where(m => m.To.AppId == user.AppId))
                    {
                        m.IsRead = true;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);

                    return Ok(new { success = true });
                }

                var alert = await db.Alerts.Where(m => m.To.AppId == user.AppId).Where(m => m.Id == id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (alert == null)
                {
                    return NotFound();
                }

                alert.IsRead = true;
                await db.SaveChangesAsync().ConfigureAwait(true);

                return Ok(new { success = true });
            }
        }

        /// <summary>
        /// Delete an alert sent to the user (authorized via API key or header)
        /// </summary>
        /// <param name="id">alert id</param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/v1/alerts/user/{id}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public async Task<IHttpActionResult> DeleteUserAlert(int? id)
        {
            using (var db = new ZapContext())
            {
                var user = await GetCurrentUser(db).ConfigureAwait(true);

                if (user == null)
                {
                    return BadRequest();
                }

                if (id == -1)
                {
                    foreach (var m in db.Alerts.Where(m => m.To.AppId == user.AppId))
                    {
                        m.IsDeleted = true;
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);

                    return Ok(new { success = true });
                }

                var alert = await db.Alerts.Where(m => m.To.AppId == user.AppId).Where(m => m.Id == id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (alert == null)
                {
                    return NotFound();
                }

                alert.IsDeleted = true;
                await db.SaveChangesAsync().ConfigureAwait(true);

                return Ok(new { success = true });
            }
        }

        private async Task<User> GetCurrentUser(ZapContext db)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.AppId == userId).ConfigureAwait(true);
            return user;
        }
    }
}