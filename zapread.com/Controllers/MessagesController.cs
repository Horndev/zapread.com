using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.API.Account;
using zapread.com.Models.Database;
using zapread.com.Models.Messages;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    public class MessagesController : Controller
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Chats()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Messages/Chats" });
            }

            return View();
        }

        public ActionResult All()
        {
            return View();
        }

        public ActionResult Alerts()
        {
            return View();
        }

        /// <summary>
        /// Queries the users for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> GetChatsTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null || dataTableParameters == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var pageUserChatsQSReceived = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Messages)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.IsPrivateMessage)
                    .Select(m => new 
                    { 
                        other = m.From.Name, 
                        otherid = m.From.AppId, 
                        toid = m.To.AppId,
                        m.IsRead,
                        m.TimeStamp,
                    });

                var pageUserChatsQSSent = db.Users
                    .SelectMany(u => u.Messages.Where(m => m.From.AppId == userId))
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.IsPrivateMessage)
                    .Select(m => new 
                    { 
                        other = m.To.Name, 
                        otherid = m.To.AppId, 
                        toid = m.To.AppId,
                        m.IsRead,
                        m.TimeStamp,
                    });

                var messageSet = pageUserChatsQSReceived.Union(pageUserChatsQSSent);

                var pageUserChatsQS = messageSet//pageUserChatsQSReceived;
                    .GroupBy(a => a.other)
                    .Select(x => x.OrderByDescending(y => y.TimeStamp).FirstOrDefault())
                    .AsQueryable();

                // Build our query
                var pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.TimeStamp);

                var sorts = dataTableParameters.Order;

                if (sorts == null)
                {
                    pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.TimeStamp);
                }
                else
                {
                    foreach (var s in sorts)
                    {
                        if (s.Dir == "asc")
                        {
                            if (dataTableParameters.Columns[s.Column].Name == "LastMessage")
                                pageUserChatsQ = pageUserChatsQS.OrderBy(q => q.TimeStamp ?? DateTime.UtcNow);
                            else if (dataTableParameters.Columns[s.Column].Name == "From")
                                pageUserChatsQ = pageUserChatsQS.OrderBy(q => q.other);
                        }
                        else
                        {
                            if (dataTableParameters.Columns[s.Column].Name == "LastMessage")
                                pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.TimeStamp);
                            else if (dataTableParameters.Columns[s.Column].Name == "From")
                                pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.other);
                        }
                    }
                }

                int numrec = await pageUserChatsQ.CountAsync().ConfigureAwait(true);

                var valuesQ = await pageUserChatsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .Select(u => new 
                    {
                        From = u.other,
                        IsRead = u.IsRead ? "Read" : "Unread",
                        u.TimeStamp,
                        FromID = u.otherid,
                        Status = u.toid == userId ? "Waiting" : "Replied",
                    })
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(true);

                // have to do this afterwords since DateTime.ToString() is not supported on SQL Server
                var values = valuesQ
                    .Select(u => new ChatsDataItem()
                    {
                        From = u.From,
                        IsRead = u.IsRead,
                        LastMessage = u.TimeStamp.HasValue ? u.TimeStamp.Value.ToString("o", CultureInfo.InvariantCulture) : "?",
                        FromID = u.FromID,
                        Status = u.Status,
                    });

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                };
                return Json(ret);
            }
        }

        public class ChatsDataItem
        {
            public string Status { get; set; }
            public string IsRead { get; set; }
            public string Type { get; set; }
            public string From { get; set; }
            public string FromID { get; set; }
            public string LastMessage { get; set; }
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public async Task<ActionResult> Unread(bool? include_alerts, bool? include_content)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, message = "Unauthorized." }, JsonRequestBehavior.AllowGet);
            }
            using (var db = new ZapContext())
            {
                var messageQuery = db.Users
                    .Where(u => u.AppId == userAppId)
                    .SelectMany(u => u.Messages)
                    .Where(m => !m.IsDeleted)
                    .Where(m => !m.IsRead);

                if (include_alerts.HasValue && !include_alerts.Value)
                {
                    messageQuery = messageQuery.
                        Where(m => m.IsPrivateMessage);
                }

                if (include_content.HasValue && include_content.Value)
                {
                    var messages = await messageQuery
                    .Select(m => new
                    {
                        MessageId = m.Id,
                        FromId = m.From.Id,
                        FromName = m.From.Name,
                        ToId = m.To.Id,
                        ToName = m.To.Name,
                        m.IsPrivateMessage,
                        m.TimeStamp,
                        m.Content,
                    })
                    .ToListAsync().ConfigureAwait(false);
                    return Json(new { success = true, messages }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var messages = await messageQuery
                    .Select(m => new
                    {
                        MessageId = m.Id,
                        FromId = m.From.Id,
                        FromName = m.From.Name,
                        ToId = m.To.Id,
                        ToName = m.To.Name,
                        m.IsPrivateMessage,
                        m.TimeStamp,
                    })
                    .ToListAsync().ConfigureAwait(false);
                    return Json(new { success = true, messages }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Queries messages for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<ActionResult> GetMessagesTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null || dataTableParameters == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;

                var pageUserMessagesQS = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Messages)
                    .Include(m => m.From)
                    .Include(m => m.PostLink)
                    .Include(m => m.CommentLink)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.CommentLink != null);

                // Build our query
                IOrderedQueryable<UserMessage> pageUserMessagesQ = null;

                foreach (var s in sorts)
                {
                    if (s.Dir == "asc")
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "Date")
                            pageUserMessagesQ = pageUserMessagesQS.OrderBy(q => q.TimeStamp ?? DateTime.UtcNow);
                        else if (dataTableParameters.Columns[s.Column].Name == "From")
                            pageUserMessagesQ = pageUserMessagesQS.OrderBy(q => q.From != null ? q.From.Name : "");
                    }
                    else
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "Date")
                            pageUserMessagesQ = pageUserMessagesQS.OrderByDescending(q => q.TimeStamp);
                        else if (dataTableParameters.Columns[s.Column].Name == "From")
                            pageUserMessagesQ = pageUserMessagesQS.OrderByDescending(q => q.From != null ? q.From.Name : "");
                    }
                }

                // Ensure default sort order
                if (pageUserMessagesQ == null)
                {
                    pageUserMessagesQ = pageUserMessagesQS.OrderByDescending(m => m.TimeStamp);
                }

                var pageUserMessages = await pageUserMessagesQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync().ConfigureAwait(true);

                var values = pageUserMessages.AsParallel()
                    .Select(u => new MessageDataItem()
                    {
                        Id = u.Id,
                        Type = u.IsPrivateMessage ? "Private Message" : u.CommentLink != null ? "Comment" : "?",
                        Date = u.TimeStamp != null ? u.TimeStamp.Value.ToString("o", CultureInfo.InvariantCulture) : "?",
                        From = u.From != null ? u.From.Name : "?",
                        FromID = u.From != null ? u.From.AppId : "?",
                        Message = u.Content,
                        Status = u.IsRead ? "Read" : "Unread",
                        Link = u.PostLink != null ? u.PostLink.PostId.ToString(CultureInfo.InvariantCulture) : "",
                        Anchor = u.CommentLink != null ? u.CommentLink.CommentId.ToString(CultureInfo.InvariantCulture) : "",
                    }).ToList();

                int numrec = await pageUserMessagesQ.CountAsync().ConfigureAwait(true);

                var ret = new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                };
                return Json(ret);
            }
        }

        /// <summary>
        /// Queries alerts for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetAlertsTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;

                var pageUserAlertsQS = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Alerts)
                    .Include(m => m.PostLink)
                    .Include(m => m.CommentLink)
                    .Where(m => !m.IsDeleted);

                // Build our query and ensure default sort order
                IOrderedQueryable<UserAlert> pageUserAlertsQ = pageUserAlertsQS.OrderByDescending(m => m.TimeStamp);

                foreach (var s in sorts)
                {
                    if (s.Dir == "asc")
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "Date")
                            pageUserAlertsQ = pageUserAlertsQS.OrderBy(q => q.TimeStamp ?? DateTime.UtcNow);
                    }
                    else
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "Date")
                            pageUserAlertsQ = pageUserAlertsQS.OrderByDescending(q => q.TimeStamp ?? DateTime.UtcNow);
                    }
                }

                var pageUserAlerts = await pageUserAlertsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync();

                List<AlertDataItem> values = GetAlertDataItems(pageUserAlerts);

                int numrec = await pageUserAlertsQ.CountAsync();

                return Json(new
                {
                    draw = dataTableParameters.Draw,
                    recordsTotal = numrec,
                    recordsFiltered = numrec,
                    data = values
                });
            }
        }

        private static List<AlertDataItem> GetAlertDataItems(List<UserAlert> pageUserAlerts)
        {
            return pageUserAlerts.AsParallel()
            .Select(u => new AlertDataItem()
            {
                AlertId = u.Id,
                Date = u.TimeStamp != null ? u.TimeStamp.Value.ToString("o") : "?",
                Title = u.Title,
                Message = u.Content,
                Status = u.IsRead ? "Read" : "Unread",
                Link = u.PostLink != null ? u.PostLink.PostId.ToString() : "",
                Anchor = u.CommentLink != null ? u.CommentLink.CommentId.ToString() : "",
                HasCommentLink = u.CommentLink != null,
                HasLink = u.PostLink != null,
            }).ToList();
        }

        public class AlertDataItem
        {
            public int AlertId { get; set; }
            public string Status { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Link { get; set; }
            public string Anchor { get; set; }
            public string Message { get; set; }
            public bool HasCommentLink { get; set; }
            public bool HasLink { get; set; }
        }

        /// <summary>
        /// Get all unread messages and alerts
        /// </summary>
        /// <returns></returns>
        // GET: Messages
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var user = db.Users
                    .Include("Alerts")
                    .Include("Messages")
                    .Include("Alerts.PostLink")
                    .Include("Messages.PostLink")
                    .Include("Messages.From")
                    .Where(u => u.AppId == userId).First();

                var messages = user.Messages
                    .Where(m => !m.IsRead && !m.IsDeleted).ToList();

                var alerts = user.Alerts.Where(m => !m.IsRead && !m.IsDeleted).ToList();

                var vm = new MessagesViewModel()
                {
                    Messages = messages,
                    Alerts = alerts,
                };

                return View(vm);
            }
        }

        [Route("Messages/LoadOlder")]
        [HttpPost]
        public async Task<JsonResult> LoadOlder(string otherId, int start, int blocks)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Json(new { success = false, message = "Unable to authenticate user." });
            }
            using (var db = new ZapContext())
            {
                var otherUserId = await db.Users
                    .Where(u => u.AppId == otherId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                var userId = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                var msgs = await GetChats(db, userId, otherUserId, blocks, start);

                string HTMLString = "";
                foreach (var mvm in msgs)
                {
                    HTMLString += RenderPartialViewToString("_PartialChatMessage", mvm);
                }

                return Json(new { success = true, HTMLString, message = "Error retreiving messages." });
            }
        }

        [Route("Messages/Chat/{username?}")]
        public async Task<ActionResult> Chat(string username)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var vm = new ChatMessagesViewModel();

                var otherUser = await db.Users
                    .Where(u => u.Name == username)
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var userId = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                vm.OtherUser = otherUser;
                vm.Messages = otherUser == null ? new List<ChatMessageViewModel> () 
                    : await GetChats(db, userId, otherUser.Id, 10, 0).ConfigureAwait(true);

                if (otherUser == null)
                {
                    vm.OtherUser = new User()
                    {
                        Name = username,
                        Id = 0,
                        AppId = ""
                    };
                } else {
                    if (otherUser.Id == userId) { // disallow chatting with yourself
                        return RedirectToAction("Index", "Home");
                    }
                }

                return View(vm);
            }
        }

        private static async Task<List<ChatMessageViewModel>> GetChats(ZapContext db, int userId, int otherUserId, int step, int start)
        {
            return await db.Messages
                .Where(m => m.IsPrivateMessage)
                .Where(m => !m.IsDeleted)
                .Where(m => (m.From.Id == otherUserId && m.To.Id == userId) ||
                            (m.From.Id == userId && m.To.Id == otherUserId))
                .OrderByDescending(m => m.TimeStamp)
                .Skip(start)
                .Take(step)
                .OrderBy(m => m.TimeStamp)
                .Select(m => new ChatMessageViewModel()
                {
                    Content = m.Content,
                    TimeStamp = m.TimeStamp.Value,
                    FromName = m.From.Name,
                    FromAppId = m.From.AppId,
                    FromProfileImgVersion = m.From.ProfileImage.Version,
                    IsReceived = m.From.Id == otherUserId
                })
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(true);
        }

        public async Task<PartialViewResult> UnreadMessages()
        {
            Response.AddHeader("X-Frame-Options", "DENY");
            var userId = User.Identity.GetUserId();
            var vm = new UnreadModel();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var numUnread = await db.Users
                        .Include("Messages")
                        .Include(u => u.ProfileImage)
                        .Where(u => u.AppId == userId)
                        .SelectMany(u => u.Messages)
                        .Where(m => !m.IsRead && !m.IsDeleted)
                        .CountAsync();

                    vm.NumUnread = numUnread;
                }
            }
            return PartialView("_UnreadMessages", model: vm);
        }

        public PartialViewResult RecentUnreadAlerts(int count)
        {
            string userId = null;
            if (User != null)
            {
                userId = User.Identity.GetUserId();
            }
            using (var db = new ZapContext())
            {
                var userAlerts = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Alerts)
                    .Where(m => !m.IsRead && !m.IsDeleted)
                    .OrderByDescending(m => m.TimeStamp)
                    .Take(count)
                    .Select(a => new UserAlertVm()
                    {
                        Id = a.Id,
                        Title = a.Title,
                        HasPostLink = a.PostLink != null,
                        PostLinkPostId = a.PostLink != null ? a.PostLink.PostId : 0,
                        PostLinkPostTitle = a.PostLink != null ? a.PostLink.PostTitle : "",
                        Content = a.Content,
                    })
                    .AsNoTracking()
                    .ToList();

                var vm = new RecentUnreadAlertsViewModel()
                {
                    AlertsVm = userAlerts,
                };

                return PartialView("_PartialRecentUnreadAlerts", model: vm);
            }
        }

        public PartialViewResult RecentUnreadMessages(int count)
        {
            string userId = null;
            if (User != null)
            {
                userId = User.Identity.GetUserId();
            }
            using (var db = new ZapContext())
            {
                var userMessages = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Messages)
                    .Where(m => !m.IsRead && !m.IsDeleted)
                    .OrderByDescending(m => m.TimeStamp)
                    .Take(count)
                    .Select(m => new UserMessageVm()
                    {
                        FromName = m.From == null ? "" : m.From.Name,
                        FromAppId = m.From == null ? "" : m.From.AppId,
                        Id = m.Id,
                        FromProfileImageVersion = m.From == null ? 0 : m.From.ProfileImage.Version,
                        IsComment = m.CommentLink != null,
                        PostId = m.PostLink == null ? 0 : m.PostLink.PostId,
                        IsPrivateMessage = m.IsPrivateMessage,
                        Content = m.Content,
                    })
                    .AsNoTracking()
                    .ToList();

                var vm = new RecentUnreadMessagesViewModel()
                {
                    MessagesVm = userMessages,
                };

                return PartialView("_PartialRecentUnreadMessages", model: vm);
            }
        }

        [HttpGet]
        public async Task<PartialViewResult> UnreadAlerts()
        {
            Response.AddHeader("X-Frame-Options", "DENY");
            string userId = null;
            if (User != null)
            {
                userId = User.Identity.GetUserId();
            }

            var vm = new UnreadModel();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var numUnread = await db.Users
                        .Include("Alerts")
                        .Where(u => u.AppId == userId)
                        .SelectMany(u => u.Alerts)
                        .Where(m => !m.IsRead && !m.IsDeleted)
                        .CountAsync();

                    vm.NumUnread = numUnread;
                }
            }
            return PartialView("_UnreadMessages", model: vm);
        }

        public async Task<JsonResult> DismissAlert(int id)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var user = db.Users
                        .Include("Alerts")
                        .Where(u => u.AppId == userId).First();

                    if (user == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    if (id == -1)
                    {
                        // dismissed all
                        foreach (var a in user.Alerts.Where(m => !m.IsDeleted && !m.IsRead))
                        {
                            a.IsRead = true;
                        }
                        await db.SaveChangesAsync();
                        return Json(new { Result = "Success" });
                    }

                    var alert = user.Alerts.Where(m => m.Id == id).FirstOrDefault();

                    if (alert == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    alert.IsRead = true;
                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" });
                }
            }

            return Json(new { Result = "Failure" });
        }

        public async Task<JsonResult> DismissMessage(int id)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var user = db.Users
                        .Include("Messages")
                        .Where(u => u.AppId == userId).First();

                    if (user == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    if (id == -1)
                    {
                        // dismissed all
                        foreach (var a in user.Messages.Where(m => !m.IsDeleted && !m.IsRead))
                        {
                            a.IsRead = true;
                        }
                        await db.SaveChangesAsync();
                        return Json(new { Result = "Success" });
                    }

                    var msg = user.Messages.Where(m => m.Id == id).FirstOrDefault();

                    if (msg == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    msg.IsRead = true;
                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" });
                }
            }

            return Json(new { Result = "Failure" });
        }

        public async Task<JsonResult> DeleteAlert(int id)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var user = await db.Users
                        .Include("Alerts")
                        .Where(u => u.AppId == userId)
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    if (id == -1)
                    {
                        // dismissed all
                        foreach (var a in user.Alerts.Where(m => !m.IsDeleted && !m.IsRead))
                        {
                            a.IsDeleted = true;
                        }
                        await db.SaveChangesAsync();
                        return Json(new { Result = "Success" });
                    }

                    var alert = user.Alerts.Where(m => m.Id == id).FirstOrDefault();

                    if (alert == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    alert.IsDeleted = true;
                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" });
                }
            }

            return Json(new { Result = "Failure" });
        }

        /// <summary>
        /// Checks if the user has any unread chats
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> CheckUnreadChats()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return Json(new { Unread = 0, success = true });
            }

            using (var db = new ZapContext())
            {
                var unreadChats = await db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Messages)
                    .Where(m => m.From != null)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.Title.StartsWith("Private") || m.IsPrivateMessage)
                    .Where(m => m.IsRead == false)
                    .CountAsync();

                return Json(new { Unread = unreadChats, success = true });
            }
        }

        public async Task<JsonResult> DeleteMessage(int id)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var user = await db.Users
                        .Include("Messages")
                        .Where(u => u.AppId == userId)
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    if (id == -1)
                    {
                        // dismissed all
                        foreach (var a in user.Messages.Where(m => !m.IsDeleted && !m.IsRead))
                        {
                            a.IsDeleted = true;
                        }
                        await db.SaveChangesAsync();
                        return Json(new { Result = "Success" });
                    }

                    var msg = user.Messages.Where(m => m.Id == id).FirstOrDefault();

                    if (msg == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    msg.IsDeleted = true;
                    await db.SaveChangesAsync();
                    return Json(new { Result = "Success" });
                }
            }

            return Json(new { Result = "Failure" });
        }

        /// <summary>
        /// Send a private message
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="isChat"></param>
        /// <returns></returns>
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> SendMessage(int id, string content, bool? isChat)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var sender = await db.Users
                        .Where(u => u.AppId == userId).FirstOrDefaultAsync().ConfigureAwait(true);

                    var receiver = await db.Users
                        .Include("Messages")
                        .Where(u => u.Id == id).FirstOrDefaultAsync().ConfigureAwait(true);

                    if (sender == null || receiver == null)
                    {
                        return Json(new { success = false, result = "Failure", message = "User not found." });
                    }

                    // Sanitize the message to prevent XSS attacks.
                    var cleanContent = content.SanitizeXSS();

                    var msg = new UserMessage()
                    {
                        IsPrivateMessage = true,
                        Content = cleanContent,
                        From = sender,
                        To = receiver,
                        IsDeleted = false,
                        IsRead = false,//(isChat != null && isChat.Value) ? true : false,
                        TimeStamp = DateTime.UtcNow,
                        Title = "Private message from <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = sender.Name }, protocol: Request.Url.Scheme) + "'>" + sender.Name + "</a>",//" + sender.Name,
                    };

                    receiver.Messages.Add(msg);
                    await db.SaveChangesAsync().ConfigureAwait(true);

                    // Live update to any listeners
                    string HTMLString = "";

                    var mvm = new ChatMessageViewModel()
                    {
                        Content = msg.Content,
                        FromAppId = msg.From.AppId,
                        FromName = msg.From.Name,
                        TimeStamp = msg.TimeStamp.Value,
                        IsReceived = true,

                        Message = msg,
                        From = msg.From,
                        To = msg.To,
                    };

                    HTMLString = RenderPartialViewToString("_PartialChatMessage", mvm);

                    // Send stream update
                    await NotificationService.SendPrivateChat(HTMLString, receiver.AppId, sender.AppId, Url.Action("Chat", "Messages", new { username = sender.Name }));

                    // Send stream update popup
                    NotificationService.SendPrivateMessage(cleanContent, receiver.AppId, "Private Message From " + sender.Name, Url.Action("Chat", "Messages", new { username = sender.Name }));

                    // email if not in chat
                    isChat = false;
                    if (isChat == null || (isChat != null && !isChat.Value))
                    {
                        // Send email
                        if (receiver.Settings != null && receiver.Settings.NotifyOnPrivateMessage)
                        {
                            string mentionedEmail = (await UserManager.FindByIdAsync(receiver.AppId).ConfigureAwait(true)).Email;

                            var mailer = DependencyResolver.Current.GetService<MailerController>();
                            mailer.ControllerContext = new ControllerContext(this.Request.RequestContext, mailer);

                            string subject = "New private ZapRead message from " + sender.Name;
                            await mailer.SendNewChat(msg.Id, mentionedEmail, subject).ConfigureAwait(true);
                        }
                    }
                    return Json(new { success = true, result = "Success", id = msg.Id });
                }
            }
            return Json(new { success = false, result = "Failure", message = "Error sending message." });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3147:Mark Verb Handlers With Validate Antiforgery Token", Justification = "<Pending>")]
        public async Task<JsonResult> GetMessage(int id)
        {
            var userAppId = User.Identity.GetUserId();

            if(userAppId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new { success = false, result = "Failure", message = "Error verifying logged in user." });
            }

            string HTMLString = "";

            using (var db = new ZapContext())
            {
                var userId = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var mvm = await db.Messages
                    .Where(m => m.Id == id)
                    .Where(m => m.From.AppId == userAppId || m.To.AppId == userAppId)       // Only fetch messages in party
                    .Select(m => new ChatMessageViewModel()
                    {
                        Content = m.Content,
                        TimeStamp = m.TimeStamp.Value,
                        FromName = m.From.Name,
                        FromAppId = m.From.AppId,
                        IsReceived = m.To.Id == userId,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                HTMLString = RenderPartialViewToString("_PartialChatMessage", mvm);

                return Json(new
                {
                    HTMLString,
                });
            }
        }

        [HttpGet, AllowAnonymous]
        public JsonResult TestMailer()
        {
            string HTMLString = "";

            using (var db = new ZapContext())
            {
                var msg = db.Messages
                    .Include("From")
                    .Include("To")
                    .SingleOrDefault(m => m.Id == 1);

                var mvm = new ChatMessageViewModel()
                {
                    Message = msg,
                    From = msg.From,
                    To = msg.To,
                    IsReceived = false,
                };
                HTMLString = RenderPartialViewToString("_PartialChatMessage", mvm);

                var result = PreMailer.Net.PreMailer.MoveCssInline(
                    baseUri: new Uri("https://www.zapread.com"),
                    html: HTMLString,
                    removeComments: true
                    );

                var msgHTML = result.Html; 		// Resultant HTML, with CSS in-lined.
                var msgWarn = String.Join(",", result.Warnings); 	// string[] of any warnings that occurred during processing.

                return Json(new
                {
                    HTMLString,
                    msgHTML,
                    msgWarn
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult =
                ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext
                (ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
