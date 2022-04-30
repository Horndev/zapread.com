using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using zapread.com.Database;
using zapread.com.Models.Subscription;
using zapread.com.Services;

namespace zapread.com.Controllers
{
    /// <summary>
    /// Controller for managing subscriptions
    /// </summary>
    public class SubscriptionController : Controller
    {
        private ApplicationUserManager _userManager;

        /// <summary>
        /// 
        /// </summary>
        public SubscriptionController()
        {
        }

        /// <summary>
        /// DI for userManager
        /// </summary>
        /// <param name="userManager"></param>
        public SubscriptionController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        /// <summary>
        /// Access the OWIN user database through the UserManager
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private void XFrameOptionsDeny()
        {
            try
            {
                Response.AddHeader("X-Frame-Options", "DENY");
            }
            catch
            {
                // TODO: add error handling - temp fix for unit test.
            }
        }

        /// <summary>
        /// Overview
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            XFrameOptionsDeny();
            var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"]; // This is our private symmetric encryption key to convert between userAppId and unsubscribeId

            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();

                var userUnsubscribeId = CryptoService.EncryptString(key, userAppId) + ":" + SubscriptionTypes.FollowedUserNewPost;

                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        u.Name
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var vm = new UnsubscribeIndexViewModel()
                {
                    Name = userInfo.Name,
                    UnsubFunction = "followed user post notifications",
                    UserEmail = UserManager.FindById(userAppId).Email,
                    UserUnsubscribeId = userUnsubscribeId,
                };
                return View(vm);
            }
        }

        /// <summary>
        /// Link to unsubscribe a user
        /// </summary>
        /// <param name="userUnsubscribeId">encrypted and uri-escaped user id and values</param>
        /// <returns></returns>
        [Route("Subscription/Unsubscribe/{userUnsubscribeId}")]
        [HttpGet]
        public async Task<ActionResult> Unsubscribe(string userUnsubscribeId)
        {
            XFrameOptionsDeny();
            var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"]; // This is our private symmetric encryption key to convert between userAppId and unsubscribeId

            // We decrypt the value in the userUnsubscribeId to get the AppId to find the user in the database.
            // This is done so that someone can't do an attack unsubscribing all users
            var unsubscribeInfo = CryptoService.DecryptString(key, userUnsubscribeId);
            var values = unsubscribeInfo.Split(':');
            var userAppId = values[0];
            var subType = values[1];

            using (var db = new ZapContext())
            {
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        u.Name
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (userInfo == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new EmptyResult();
                }

                string function = "";

                switch (subType)
                {
                    case SubscriptionTypes.FollowedUserNewPost:
                        function = "followed user post notifications";
                        break;
                    case "B":
                        function = "followed post notifications";
                        break;
                    case SubscriptionTypes.OwnPostComment:
                        function = "comments on your posts";
                        break;
                    case SubscriptionTypes.FollowedPostComment:
                        function = "comments on folllowed posts";
                        break;
                    case SubscriptionTypes.OwnCommentReply:
                        function = "reply to comments";
                        break;
                    case SubscriptionTypes.NewChat:
                        function = "new private chats";
                        break;
                    case SubscriptionTypes.UserMentionedInComment:
                        function = "user mentions";
                        break;
                    default:
                        break;
                }

                var vm = new UnsubscribeIndexViewModel()
                {
                    Name = userInfo.Name,
                    UnsubFunction = function,
                    UserEmail = UserManager.FindById(userAppId).Email,
                    UserUnsubscribeId = userUnsubscribeId
                };

                return View(vm);
            }
        }

        /// <summary>
        /// Do the unsubscribe
        /// </summary>
        /// <param name="userUnsubscribeId"></param>
        /// <returns></returns>
        [Route("Subscription/Confirm/{userUnsubscribeId}")]
        [HttpGet]
        public async Task<ActionResult> Confirm(string userUnsubscribeId)
        {
            XFrameOptionsDeny();
            var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"]; // This is our private symmetric encryption key to convert between userAppId and unsubscribeId

            // We decrypt the value in the userUnsubscribeId to get the AppId to find the user in the database.
            // This is done so that someone can't do an attack unsubscribing all users
            var unsubscribeInfo = CryptoService.DecryptString(key, userUnsubscribeId);
            var values = unsubscribeInfo.Split(':');
            var userAppId = values[0];
            var subscriptionType = values[1];
            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Include(u => u.Settings)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (user == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new EmptyResult();
                }

                string function = "";
                bool success = false;

                switch(subscriptionType)
                {
                    case SubscriptionTypes.FollowedUserNewPost:
                        function = "followed user post notifications";
                        user.Settings.NotifyOnNewPostSubscribedUser = false;
                        success = true;
                        break;
                    case SubscriptionTypes.OwnPostComment:
                        function = "comments on your posts";
                        user.Settings.NotifyOnOwnPostCommented = false;
                        break;
                    case SubscriptionTypes.FollowedPostComment:
                        function = "comments on folllowed posts";
                        // TODO
                        break;
                    case SubscriptionTypes.OwnCommentReply:
                        function = "reply to comments";
                        user.Settings.NotifyOnOwnCommentReplied = false;
                        break;
                    case SubscriptionTypes.NewChat:
                        function = "new private chats";
                        user.Settings.NotifyOnPrivateMessage = false;
                        break;
                    case SubscriptionTypes.UserMentionedInComment:
                        function = "user mentions";
                        user.Settings.NotifyOnMentioned = false;
                        break;
                    default:
                        break;
                }
                try
                {
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    success = false;
                }
                
                var vm = new UnsubscribeIndexViewModel()
                {
                    Success = success,
                    Name = user.Name,
                    UnsubFunction = function,
                    UserEmail = UserManager.FindById(userAppId).Email,
                    UserUnsubscribeId = userUnsubscribeId
                };

                return View(vm);
            }
        }

        /// <summary>
        /// Re-subscribe
        /// </summary>
        /// <param name="userUnsubscribeId"></param>
        /// <returns></returns>
        [Route("Subscription/Subscribe/{userUnsubscribeId}")]
        [HttpGet]
        public async Task<ActionResult> Subscribe(string userUnsubscribeId)
        {
            XFrameOptionsDeny();
            var key = System.Configuration.ConfigurationManager.AppSettings["UnsubscribeKey"]; // This is our private symmetric encryption key to convert between userAppId and unsubscribeId

            // We decrypt the value in the userUnsubscribeId to get the AppId to find the user in the database.
            // This is done so that someone can't do an attack unsubscribing all users
            var unsubscribeInfo = CryptoService.DecryptString(key, userUnsubscribeId);
            var values = unsubscribeInfo.Split(':');
            var userAppId = values[0];
            var subscriptionType = values[1];

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Include(u => u.Settings)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (user == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return new EmptyResult();
                }

                string function = "";
                bool success = false;

                switch (subscriptionType)
                {
                    case SubscriptionTypes.FollowedUserNewPost:
                        function = "followed user post notifications";
                        user.Settings.NotifyOnNewPostSubscribedUser = true;
                        success = true;
                        break;
                    case SubscriptionTypes.OwnPostComment:
                        function = "comments on your posts";
                        user.Settings.NotifyOnOwnPostCommented = true;
                        break;
                    case SubscriptionTypes.FollowedPostComment:
                        function = "comments on folllowed posts";
                        // TODO
                        break;
                    case SubscriptionTypes.OwnCommentReply:
                        function = "reply to comments";
                        user.Settings.NotifyOnOwnCommentReplied = true;
                        break;
                    case SubscriptionTypes.NewChat:
                        function = "new private chats";
                        user.Settings.NotifyOnPrivateMessage = true;
                        break;
                    case SubscriptionTypes.UserMentionedInComment:
                        function = "user mentions";
                        user.Settings.NotifyOnMentioned = true;
                        break;
                    default:
                        break;
                }
                try
                {
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    success = false;
                }

                var vm = new UnsubscribeIndexViewModel()
                {
                    Success = success,
                    Name = user.Name,
                    UnsubFunction = function,
                    UserEmail = UserManager.FindById(userAppId).Email,
                    UserUnsubscribeId = userUnsubscribeId
                };

                return View(vm);
            }
        }
    }
}