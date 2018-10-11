using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models;
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
                    .Where(u => u.AppId == userId).First();

                var messages = user.Messages.Where(m => !m.IsRead && !m.IsDeleted).ToList();

                var alerts = user.Alerts.Where(m => !m.IsRead && !m.IsDeleted).ToList();

                var vm = new MessagesViewModel()
                {
                    Messages = messages,
                    Alerts = alerts,
                };

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
                        //.Include("Messages.PostLink")
                        .Where(u => u.AppId == userId).First();

                    var messages = user.Messages.Where(m => !m.IsRead && !m.IsDeleted).OrderByDescending(m => m.TimeStamp);

                    vm.NumUnread = messages.Count();
                }
            }
            return PartialView("_UnreadMessages", model: vm);
        }

        public PartialViewResult UnreadAlerts()
        {
            var userId = User.Identity.GetUserId();
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

        public async Task<JsonResult> SendMessage(int id, string content)
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
                        Content = content,
                        From = sender,
                        To = receiver,
                        IsDeleted = false,
                        IsRead = false,
                        TimeStamp = DateTime.UtcNow,
                        Title = "Private message from " + sender.Name,
                    };

                    receiver.Messages.Add(msg);
                    await db.SaveChangesAsync();

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
                                Body = "From: " + sender.Name + "<br/> " + content + "<br/><br/><a href='http://www.zapread.com'>zapread.com</a>",
                                Destination = mentionedEmail,
                                Email = "",
                                Name = "ZapRead.com Notify"
                            });
                    }
                    
                    return Json(new { Result = "Success" });
                }
            }

            return Json(new { Result = "Failure" });
        }
    }
}