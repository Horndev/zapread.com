using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Services;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

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
        public async Task<ActionResult> Chats()
        {
            return View();
        }

        public async Task<ActionResult> All()
        {
            return View();
        }

        public async Task<ActionResult> Alerts()
        {
            return View();
        }

        /// <summary>
        /// Queries the users for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetChatsTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var sorts = dataTableParameters.Order;

                var pageUserChatsQSReceived = db.Users
                    .Where(u => u.AppId == userId)
                    .SelectMany(u => u.Messages)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.IsPrivateMessage)
                    .Include(m => m.From)
                    .Include(m => m.To)
                    .Select(m => new { other = m.From.Name, otherid = m.From.AppId, toid = m.To.AppId, m });

                var pageUserChatsQSSent = db.Users
                    .SelectMany(u => u.Messages.Where(m => m.From.AppId == userId))
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.IsPrivateMessage)
                    .Include(m => m.To)
                    .Include(m => m.From)
                    .Select(m => new { other = m.To.Name, otherid = m.To.AppId, toid = m.To.AppId, m });

                var messageSet = pageUserChatsQSReceived.Union(pageUserChatsQSSent);

                var pageUserChatsQS = messageSet//pageUserChatsQSReceived;
                    .GroupBy(a => a.other)
                    .Select(x => x.OrderByDescending(y => y.m.TimeStamp).FirstOrDefault())
                    .AsQueryable();

                var cq = pageUserChatsQS.ToList();

                // Build our query
                var pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.m.TimeStamp); ;

                foreach (var s in sorts)
                {
                    if (s.Dir == "asc")
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "LastMessage")
                            pageUserChatsQ = pageUserChatsQS.OrderBy(q => q.m.TimeStamp ?? DateTime.UtcNow);
                        else if (dataTableParameters.Columns[s.Column].Name == "From")
                            pageUserChatsQ = pageUserChatsQS.OrderBy(q => q.other);
                    }
                    else
                    {
                        if (dataTableParameters.Columns[s.Column].Name == "LastMessage")
                            pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.m.TimeStamp);
                        else if (dataTableParameters.Columns[s.Column].Name == "From")
                            pageUserChatsQ = pageUserChatsQS.OrderByDescending(q => q.other);
                    }
                }

                var pageUserChats = await pageUserChatsQ
                    .Skip(dataTableParameters.Start)
                    .Take(dataTableParameters.Length)
                    .ToListAsync();

                var values = pageUserChats.AsParallel()
                    .Select(u => new ChatsDataItem()
                    {
                        From = u.other,
                        LastMessage = u.m.TimeStamp.HasValue ? u.m.TimeStamp.Value.ToString("o") : "?",
                        FromID = u.otherid,
                        Status = u.toid == userId ? "Waiting" : "Replied",
                    }).ToList();

                int numrec = await pageUserChatsQ.CountAsync();

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
            public string Type { get; set; }
            public string From { get; set; }
            public string FromID { get; set; }
            public string LastMessage { get; set; }
        }

        /// <summary>
        /// Queries messages for a paging table.
        /// </summary>
        /// <param name="dataTableParameters"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetMessagesTable([System.Web.Http.FromBody] DataTableParameters dataTableParameters)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
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
                    .ToListAsync();

                var values = pageUserMessages.AsParallel()
                    .Select(u => new MessageDataItem()
                    {
                        Type = u.IsPrivateMessage ? "Private Message" : u.CommentLink != null ? "Comment" : "?",
                        Date = u.TimeStamp != null ? u.TimeStamp.Value.ToString("o") : "?",
                        From = u.From != null ? u.From.Name : "?",
                        FromID = u.From != null ? u.From.AppId : "?",
                        Message = u.Content,
                        Status = u.IsRead ? "Read" : "Unread",
                        Link = u.PostLink != null ? u.PostLink.PostId.ToString() : "",
                        Anchor = u.CommentLink !=null ? u.CommentLink.CommentId.ToString() : "",
                    }).ToList();

                int numrec = await pageUserMessagesQ.CountAsync();

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

        public class MessageDataItem
        {
            public string Status { get; set; }
            public string Type { get; set; }
            public string From { get; set; }
            public string FromID { get; set; }
            public string Date { get; set; }
            public string Link { get; set; }
            public string Anchor { get; set; }
            public string Message { get; set; } 
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

        [Route("Messages/Chat/{username?}")]
        public async Task<ActionResult> Chat(string username)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new ZapContext())
            {
                var vm = new ChatMessagesViewModel();

                var user = await db.Users
                    .Include("Messages")
                    .Include("Messages.PostLink")
                    .Include("Messages.From")
                    .Where(u => u.AppId == userId)
                    .SingleOrDefaultAsync();

                var otheruser = await db.Users
                    .Include("Messages")
                    .Include("Messages.PostLink")
                    .Include("Messages.From")
                    .Where(u => u.Name == username)
                    .SingleOrDefaultAsync();

                if (otheruser == null)
                {
                    return RedirectToAction("Index", "Messages");
                }

                int thisUserId = user.Id;
                int otherUserId = otheruser.Id;

                // Better to just search from & to?

                var receivedMessages = user.Messages
                    .Where(m => m.From != null && m.From.Id == otherUserId)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.Title.StartsWith("Private") || m.IsPrivateMessage).ToList();

                var sentMessages = otheruser.Messages
                    .Where(m => m.From != null && m.From.Id == thisUserId)
                    .Where(m => !m.IsDeleted)
                    .Where(m => m.Title.StartsWith("Private") || m.IsPrivateMessage).ToList();

                var messages = new List<ChatMessageViewModel>();

                foreach (var m in receivedMessages)
                {
                    messages.Add(new ChatMessageViewModel()
                    {
                        Message = m,
                        From = otheruser,
                        To = user,
                        IsReceived = true,
                    });
                }

                foreach (var m in sentMessages)
                {
                    messages.Add(new ChatMessageViewModel()
                    {
                        Message = m,
                        From = user,
                        To = otheruser,
                        IsReceived = false,
                    });
                }

                vm.OtherUser = otheruser;
                vm.ThisUser = user;
                vm.Messages = messages
                    .OrderByDescending(mv => mv.Message.TimeStamp)
                    .Take(10)
                    .OrderBy(mv => mv.Message.TimeStamp)
                    .ToList();

                return View(vm);
            }
        }

        public PartialViewResult UnreadMessages()
        {
            var userId = User.Identity.GetUserId();
            var vm = new UnreadModel();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var user = db.Users
                        //.Include("Alerts")
                        .Include("Messages")
                        //.Include("Alerts.PostLink")
                        .Include("Messages.PostLink")
                        .Where(u => u.AppId == userId).First();

                    var messages = user.Messages.Where(m => !m.IsRead && !m.IsDeleted).OrderByDescending(m => m.TimeStamp);

                    vm.NumUnread = messages.Count();
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
                var user = db.Users
                    .Include("Alerts")
                    //.Include("Messages")
                    .Include("Alerts.PostLink")
                    //.Include("Messages.PostLink")
                    //.Include("Messages.From")
                    .Where(u => u.AppId == userId).FirstOrDefault();

                var vm = new RecentUnreadAlertsViewModel();

                if (user != null)
                {
                    var alerts = user.Alerts.Where(m => !m.IsRead && !m.IsDeleted).OrderByDescending(m => m.TimeStamp).Take(count);
                    vm.Alerts = alerts.ToList();
                }

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
                var user = db.Users
                    .Include("Messages")
                    .Include("Messages.CommentLink")
                    .Include("Messages.PostLink")
                    .Include("Messages.From")
                    .Where(u => u.AppId == userId)
                    .AsNoTracking()
                    .FirstOrDefault();

                var vm = new RecentUnreadMessagesViewModel();

                if (user != null)
                {
                    var messages = user.Messages.Where(m => !m.IsRead && !m.IsDeleted).OrderByDescending(m => m.TimeStamp).Take(count);
                    vm.Messages = messages.ToList();
                }

                return PartialView("_PartialRecentUnreadMessages", model: vm);
            }
        }

        public PartialViewResult UnreadAlerts()
        {
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
                    var user = db.Users
                        .Include("Alerts")
                        //.Include("Messages")
                        //.Include("Alerts.PostLink")
                        //.Include("Messages.PostLink")
                        .Where(u => u.AppId == userId).First();

                    var messages = user.Alerts.Where(m => !m.IsRead && !m.IsDeleted).OrderByDescending(m => m.TimeStamp);

                    vm.NumUnread = messages.Count();
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
                        foreach(var a in user.Alerts.Where(m => !m.IsDeleted && !m.IsRead))
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
        /// <returns></returns>
        public async Task<JsonResult> SendMessage(int id, string content, bool? isChat)
        {
            var userId = User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ZapContext())
                {
                    var sender = await db.Users
                        .Where(u => u.AppId == userId).FirstOrDefaultAsync();

                    var receiver = await db.Users
                        .Include("Messages")
                        .Where(u => u.Id == id).FirstOrDefaultAsync();

                    if (sender == null)
                    {
                        return Json(new { Result = "Failure" });
                    }

                    var msg = new UserMessage()
                    {
                        IsPrivateMessage = true, 
                        Content = content,
                        From = sender,
                        To = receiver,
                        IsDeleted = false,
                        IsRead = (isChat != null && isChat.Value) ? true : false,
                        TimeStamp = DateTime.UtcNow,
                        Title = "Private message from <a href='" + @Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = sender.Name }) + "'>" + sender.Name + "</a>",//" + sender.Name,
                    };

                    receiver.Messages.Add(msg);
                    await db.SaveChangesAsync();

                    // Live update to any listeners
                    string HTMLString = "";

                    var mvm = new ChatMessageViewModel()
                    {
                        Message = msg,
                        From = msg.From,
                        To = msg.To,
                        IsReceived = true,
                    };

                    HTMLString = RenderPartialViewToString("_PartialChatMessage", mvm);

                    // Send stream update
                    NotificationService.SendPrivateChat(HTMLString, receiver.AppId, sender.AppId, Url.Action("Chat", "Messages", new { username = sender.Name }));
                    
                    // Send stream update popup
                    NotificationService.SendPrivateMessage(content, receiver.AppId, "Private Message From " + sender.Name, Url.Action("Chat", "Messages", new { username = sender.Name }));

                    // email if not in chat
                    isChat = false;
                    if (isChat == null || (isChat != null && !isChat.Value))
                    {
                        // Send email
                        if (receiver.Settings == null)
                        {
                            receiver.Settings = new UserSettings();
                        }

                        if (receiver.Settings.NotifyOnPrivateMessage)
                        {
                            string mentionedEmail = UserManager.FindById(receiver.AppId).Email;
                            MailingService.Send(user: "Notify",
                                message: new UserEmailModel()
                                {
                                    Subject = "New private message",
                                    Body = "From: <a href='" + Url.Action(actionName: "Index", controllerName: "User", routeValues: new { username = sender.Name }) + "'>" 
                                        + sender.Name + "</a><br/> " + content 
                                        + "<br/><a href='https://www.zapread.com/Messages/Chat/" + Url.Encode(sender.Name) + "'>Go to live chat.</a>"
                                        + "<br/><br/><a href='https://www.zapread.com'>zapread.com</a>",
                                    Destination = mentionedEmail,
                                    Email = "",
                                    Name = "ZapRead.com Notify"
                                });
                        }
                    }
                    return Json(new { Result = "Success", Id = msg.Id });
                }
            }

            return Json(new { Result = "Failure" });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetMessage(int id, int userId)
        {
            string HTMLString = "";

            using (var db = new ZapContext())
            {
                var msg = db.Messages
                    .Include("From")
                    .Include("To")
                    .SingleOrDefault(m => m.Id == id);

                var mvm = new ChatMessageViewModel()
                {
                    Message = msg,
                    From = msg.From,
                    To = msg.To,
                    IsReceived = msg.To.Id == userId,
                };

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