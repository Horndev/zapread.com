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
                    var sender = db.Users
                        .Where(u => u.AppId == userId).FirstOrDefault();

                    var receiver = db.Users
                        .Include("Messages")
                        .Where(u => u.Id == id).FirstOrDefault();

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

                    NotificationService.SendPrivateChat(HTMLString, receiver.AppId, sender.AppId, Url.Action("Chat", "Messages", new { username = sender.Name }));
                    
                    // Send popup and email if not in chat
                    NotificationService.SendPrivateMessage(content, receiver.AppId, "Private Message From " + sender.Name, Url.Action("Chat", "Messages", new { username = sender.Name }));

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