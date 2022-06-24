using Hangfire;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.API;
using zapread.com.Models.API.User;
using zapread.com.Models.Database;
using zapread.com.Services;

namespace zapread.com.API
{
    /// <summary>
    /// API for ZapRead users
    /// </summary>
    public class UserController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/banneralerts/dismiss/{id}")]
        public async Task<IHttpActionResult> DismissBannerAlert(int id)
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var timeNow = DateTime.UtcNow;

                var bannerInfo = await db.BannerAlerts
                    .Where(b => b.Id == id)
                    .Select(b => new
                    {
                        Banner = b,
                        IsDismissed = b.DismissedBy.Select(u => u.AppId).Contains(userAppId)
                    })
                    .FirstOrDefaultAsync();

                if (bannerInfo == null) return NotFound();

                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync();

                if (user == null) return NotFound();

                if (!bannerInfo.IsDismissed)
                {
                    bannerInfo.Banner.DismissedBy = new List<User> { user };

                    await db.SaveChangesAsync();
                }

                return Ok(new ZapReadResponse() { success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/banneralerts/snooze/{id}")]
        public async Task<IHttpActionResult> SnoozeBannerAlert(int id)
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var timeNow = DateTime.UtcNow;

                var banner = await db.BannerAlerts
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (banner == null) return NotFound();

                if (!banner.IsGlobalSend)
                {
                    banner.StartTime = timeNow + TimeSpan.FromDays(1);
                }

                await db.SaveChangesAsync();

                return Ok(new ZapReadResponse() { success=true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/user/banneralerts")]
        public async Task<IHttpActionResult> GetBannerAlerts()
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var timeNow = DateTime.UtcNow;

                var banners = await db.BannerAlerts
                    .Where(a => a.IsGlobalSend || (a.User != null && a.User.AppId == userAppId))
                    .Where(a => !a.DismissedBy.Select(u => u.AppId).Contains(userAppId))
                    .Where(a => a.StartTime == null || (a.StartTime != null && a.StartTime.Value < timeNow)) // Only if active
                    .Where(a => a.DeleteTime == null || (a.DeleteTime != null && a.DeleteTime.Value > timeNow)) // Only if active
                    .Select(a => new BannerAlertItem()
                    {
                        Id = a.Id,
                        Priority = a.Priority,
                        Text = a.Text,
                        Title = a.Title,
                        IsGlobalSend = a.IsGlobalSend,
                    })
                    .ToListAsync();

                return Ok(new GetBannerAlertsResponse()
                {
                    Alerts = banners,
                    success = true
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/user/reactions/list/{postId}")]
        public async Task<IHttpActionResult> GetReactions(int postId)
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Ok(new GetReactionsResponse()
                {
                    Reactions = new List<ReactionItem>()
                });
            }

            using (var db = new ZapContext())
            {
                var userPostReactions = db.Posts
                    .Where(p => p.PostId == postId)
                    .SelectMany(p => p.PostReactions.Where(r => r.User.AppId == userAppId))
                    .Select(r => r.Reaction.ReactionId);
                    //.ToListAsync().ConfigureAwait(true);

                // Most commonly used reactions will appear first
                var commonReactions = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .SelectMany(u => u.PostReactions)
                    .GroupBy(pr => pr.Reaction)
                    .Select(prg => new
                    {
                        Count = prg.Count(),
                        Reaction = prg.Key
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .Select(i => new ReactionItem()
                    {
                        Description = i.Reaction.Description,
                        ReactionIcon = i.Reaction.ReactionIcon,
                        ReactionName = i.Reaction.ReactionName,
                        ReactionId = i.Reaction.ReactionId,
                        IsApplied = userPostReactions.Contains(i.Reaction.ReactionId)
                    }).ToListAsync().ConfigureAwait(true);

                var availableReactions = db.Users
                    .Where(u => u.AppId == userAppId)
                    .SelectMany(u => u.AvailableReactions);

                var universalReactions = db.Reactions
                    .Where(r => r.UnlockedAll);

                var reactions = await availableReactions
                    .Union(universalReactions)
                    .Distinct()
                    .Select(i => new ReactionItem()
                    {
                        Description = i.Description,
                        ReactionIcon = i.ReactionIcon,
                        ReactionName = i.ReactionName,
                        ReactionId = i.ReactionId,
                        IsApplied = userPostReactions.Contains(i.ReactionId)
                    })
                    .ToListAsync().ConfigureAwait(true);  

                return Ok(new GetReactionsResponse()
                {
                    Reactions = reactions,
                    CommonReactions = commonReactions
                });
            }
        }

        /// <summary>
        /// Find a user
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/search")]
        public async Task<IHttpActionResult> Search(UserSearchRequest req)
        {
            if (req == null || req.Prefix == null || req.Max < 1)
            {
                return BadRequest();
            }

            using (var db = new ZapContext())
            {
                var users = await db.Users
                    .Where(u => u.Name.Contains(req.Prefix))
                    .OrderByDescending(u => u.DateLastActivity)
                    .Take(req.Max)
                    .Select(u => new UserResultInfo()
                    {
                        UserName = u.Name,
                        UserAppId = u.AppId,
                        ProfileImageVersion = u.ProfileImage != null ? u.ProfileImage.Version : 0
                    }).ToListAsync().ConfigureAwait(false);

                return Ok(new UserSearchResponse() { Users = users });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/follow")]
        public async Task<IHttpActionResult> Follow(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsFollowing = u.Following.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && !requestingUserInfo.IsFollowing)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    // This adds the user without hitting the db to query user list
                    requestingUserInfo.User.Following = new List<User>() { followUser };

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/unfollow")]
        public async Task<IHttpActionResult> UnFollow(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsFollowing = u.Following.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && requestingUserInfo.IsFollowing)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    requestingUserInfo.User.Following.Remove(followUser);

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/block")]
        public async Task<IHttpActionResult> Block(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsBlocking = u.BlockingUsers.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && !requestingUserInfo.IsBlocking)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    // This adds the user without hitting the db to query user list
                    requestingUserInfo.User.BlockingUsers = new List<User>() { followUser };

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/unblock")]
        public async Task<IHttpActionResult> UnBlock(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsBlocking = u.BlockingUsers.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && requestingUserInfo.IsBlocking)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    requestingUserInfo.User.BlockingUsers.Remove(followUser);

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/ignore")]
        public async Task<IHttpActionResult> Ignore(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsIgnoring = u.IgnoringUsers.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && !requestingUserInfo.IsIgnoring)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    // This adds the user without hitting the db to query user list
                    requestingUserInfo.User.IgnoringUsers = new List<User>() { followUser };

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/unignore")]
        public async Task<IHttpActionResult> UnIgnore(UserInteractionRequest req)
        {
            if (string.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var requestingUserInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        User = u,
                        IsIgnoring = u.IgnoringUsers.Select(f => f.AppId).Contains(req.UserAppId)
                    })
                    .FirstOrDefaultAsync();

                if (requestingUserInfo != null && requestingUserInfo.IsIgnoring)
                {
                    var followUser = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .FirstOrDefaultAsync();

                    if (followUser == null) return BadRequest();

                    requestingUserInfo.User.IgnoringUsers.Remove(followUser);

                    await db.SaveChangesAsync();

                    return Ok(new ZapReadResponse() { success = true });
                }

                return Ok(new ZapReadResponse() { success = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [Route("api/v1/user/info/{appId?}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetUserInfo(string appId)
        {
            using (var db = new ZapContext())
            {
                var userAppId = User.Identity.GetUserId();

                if (!string.IsNullOrEmpty(appId))
                {
                    userAppId = await db.Users
                        .Where(u => u.AppId == appId)
                        .Select(u => u.AppId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }

                if (userAppId == null) return BadRequest();

                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new { 
                        u.Name,
                        UserProfileImageVersion = u.ProfileImage.Version,
                        u.Reputation,
                        u.AboutMe,
                        UserAchievements = u.Achievements.Select(ach => new UserAchievementViewModel()
                        {
                            Id = ach.Id,
                            ImageId = ach.Achievement.Id,
                            Name = ach.Achievement.Name,
                            Description = ach.Achievement.Description,
                            DateAchieved = ach.DateAchieved.Value,
                        }),
                        NumPosts = u.Posts.Where(p => !p.IsDeleted).Where(p => !p.IsDraft).Count(),
                        NumFollowing = u.Following.Count,
                        NumFollowers = u.Followers.Count,
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return Ok(new GetUserInfoResponse()
                {
                    success = true,
                    Name = userInfo.Name,
                    Reputation = userInfo.Reputation,
                    UserProfileImageVersion = userInfo.UserProfileImageVersion,
                    AboutMe = userInfo.AboutMe,
                    Achievements = userInfo.UserAchievements.ToList(),
                    NumFollowers = userInfo.NumFollowers,
                    NumFollowing = userInfo.NumFollowing,
                    NumPosts = userInfo.NumPosts
                });
            }
        }

        /// <summary>
        /// Get user settings info
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/settings/notification")]
        public async Task<IHttpActionResult> GetUserNotificationInfo()
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return BadRequest();

            using (var db = new ZapContext())
            {
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        u.Settings,
                        u.Languages
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (userInfo == null) return NotFound();

                return Ok(new GetUserNotificationInfoResponse()
                {
                    success = true,
                    Settings = userInfo.Settings,
                    Languages = userInfo.Languages == null ? new List<string>() : userInfo.Languages.Split(',').ToList(),
                    KnownLanguages = LanguageHelpers.GetLanguages(),
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/v1/user/interaction/{appId?}")]
        public async Task<IHttpActionResult> GetUserInteractionInfo(string appId)
        {
            var userAppId = User.Identity.GetUserId();
            
            if (userAppId == null || appId == null) return BadRequest();

            using (var db = new ZapContext())
            {
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        isFollowing = u.Following.Select(us => us.AppId).Contains(appId),
                        isIgnoring = u.IgnoringUsers.Select(us => us.AppId).Contains(appId),
                        isBlocking = u.BlockingUsers.Select(us => us.AppId).Contains(appId),
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (userInfo == null) return NotFound();

                return Ok(new GetUserInteractionInfo()
                {
                    success = true,
                    IsBlocking = userInfo.isBlocking,
                    IsFollowing = userInfo.isFollowing,
                    IsIgnoring = userInfo.isIgnoring
                });
            }
        }

        /// <summary>
        /// Get user settings info
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [Route("api/v1/user/followinfo/{appId?}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetUserFollowInfo(string appId)
        {
            var userAppId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(appId)) userAppId = appId;

            if (userAppId == null) return BadRequest();

            using (var db = new ZapContext())
            {
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        u.Name,
                        TopFollowing = u.Following.OrderByDescending(us => us.TotalEarned).Take(20)
                            .Select(us => new UserFollowView()
                            {
                                Name = us.Name,
                                AppId = us.AppId,
                                ProfileImageVersion = us.ProfileImage.Version,
                            }),
                        TopFollowers = u.Followers.OrderByDescending(us => us.TotalEarned).Take(20)
                            .Select(us => new UserFollowView()
                            {
                                Name = us.Name,
                                AppId = us.AppId,
                                ProfileImageVersion = us.ProfileImage.Version,
                            }),
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (userInfo == null) return NotFound();

                return Ok(new GetUserFollowInfoResponse()
                {
                    success = true,
                    TopFollowers = userInfo.TopFollowers,
                    TopFollowing = userInfo.TopFollowing,
                    UserName = userInfo.Name,
                });
            }
        }

        /// <summary>
        /// Get user settings info
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [Route("api/v1/user/groupinfo/{appId?}")]
        public async Task<IHttpActionResult> GetUserGroupInfo(string appId)
        {
            var userAppId = User.Identity.GetUserId();

            if (!string.IsNullOrEmpty(appId)) { userAppId = appId; }

            if (userAppId == null) return BadRequest();

            using (var db = new ZapContext())
            {
                var userInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => new
                    {
                        UserGroups = u.Groups
                            .Select(g => new GroupInfo()
                            {
                                Id = g.GroupId,
                                Name = g.GroupName,
                                Icon = "fa-bolt",
                                Level = 1,
                                Progress = 36,
                                NumPosts = g.Posts.Where(p => !(p.IsDeleted || p.IsDraft)).Count(),
                                UserPosts = g.Posts.Where(p => !(p.IsDeleted || p.IsDraft)).Where(p => p.UserId.Id == u.Id).Count(),
                                IsMod = g.Moderators.Select(usr => usr.Id).Contains(u.Id),
                                IsAdmin = g.Administrators.Select(usr => usr.Id).Contains(u.Id),
                            }),
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (userInfo == null) return NotFound();

                return Ok(new GetUserGroupInfoResponse()
                {
                    success = true,
                    Groups = userInfo.UserGroups
                });
            }
        }

        /// <summary>
        /// Get user security info
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/settings/security")]
        public async Task<IHttpActionResult> GetUserSecurityInfo()
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return BadRequest();

            using (var db = new ZapContext())
            {
                using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                {
                    return Ok(new GetUserSecurityInfoResponse()
                    {
                        success = true,
                        EmailConfirmed = await userManager.IsEmailConfirmedAsync(userAppId).ConfigureAwait(true),
                        IsGoogleAuthenticatorEnabled = await userManager.IsGoogleAuthenticatorEnabledAsync(userAppId).ConfigureAwait(true),
                        TwoFactor = await userManager.GetTwoFactorEnabledAsync(userAppId).ConfigureAwait(true),
                        IsEmailAuthenticatorEnabled = await userManager.IsEmailAuthenticatorEnabledAsync(userAppId).ConfigureAwait(true),
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/v1/user/feed")]
        public async Task<IHttpActionResult> GetPosts(GetUserFeedRequest req)
        {
            if (req == null) { return BadRequest(); }

            using (var db = new ZapContext())
            {
                string userAppId = null;

                if (!string.IsNullOrEmpty(req.UserAppId))
                {
                    userAppId = await db.Users
                        .Where(u => u.AppId == req.UserAppId)
                        .Select(u => u.AppId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync().ConfigureAwait(true);
                }

                int BlockSize = req.BlockSize ?? 10;

                int BlockNumber = req.BlockNumber ?? 0;

                List<PostViewModel> postsVm = await QueryHelpers.QueryActivityPostsVm(BlockNumber, BlockSize, userAppId).ConfigureAwait(true);

                // Make images lazy TODO: apply this when submitting new posts
                postsVm.ForEach(post =>
                {
                    HtmlDocument postDocument = new HtmlDocument();
                    postDocument.LoadHtml(post.Content);

                    var postImages = postDocument.DocumentNode.SelectNodes("//img/@src");
                    if (postImages != null)
                    {
                        foreach (var postImage in postImages)
                        {
                            postImage.SetAttributeValue("loading", "lazy");
                        }
                        post.Content = postDocument.DocumentNode.OuterHtml;
                    }

                    post.PostIdEnc = zapread.com.Services.CryptoService.IntIdToString(post.PostId);
                    post.PostTitleEnc = !String.IsNullOrEmpty(post.PostTitle) ? post.PostTitle.MakeURLFriendly() : (post.UserName + " posted in " + post.GroupName).MakeURLFriendly();
                });

                var response = new Models.API.Post.GetPostsResponse()
                {
                    HasMorePosts = postsVm.Count >= BlockSize,
                    Posts = postsVm,
                    success = true,
                };

                return Ok(response);
            }
        }

        /// <summary>
        /// Get the user balance (as determined by API key used)
        /// </summary>
        /// <returns>UserBalanceResponse</returns>
        [Route("api/v1/user/balance")]
        [AcceptVerbs("GET")]
        [Authorize(Roles = "Administrator,APIUser")]
        public async Task<UserBalanceResponse> Balance()
        {
            double userBalance = 0.0;
            userBalance = await GetUserBalance().ConfigureAwait(true);

            string balance = userBalance.ToString("0.##", CultureInfo.InvariantCulture);

            HttpContext.Current.Response.Headers.Add("X-Frame-Options", "DENY");
            return new UserBalanceResponse() { success = true, balance = balance };
        }

        /// <summary>
        /// Gets statistics on the user referrals
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/referralstats")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetReferralStats()
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var referralInfo = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Where(u => u.ReferralInfo != null)
                    .Select(u => u.ReferralInfo)
                    .AsNoTracking()
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(true);

                var referredInfos = await db.Users
                       .Where(u => u.ReferralInfo != null && u.ReferralInfo.ReferredByAppId == userAppId)
                       .Select(u => u.ReferralInfo)
                       .AsNoTracking()
                       .ToListAsync().ConfigureAwait(true);

                // Check how many active and total
                var dateNow = DateTime.UtcNow;
                var numActive = referredInfos.Where(r => (dateNow - r.TimeStamp) < TimeSpan.FromDays(6 * 30)).Count();
                var numTotal = referredInfos.Count() - numActive;

                return Ok(new GetRefStatsResponse()
                {
                    CanGiftReferral = referralInfo != null && referralInfo.ReferredByAppId != null,
                    ReferredByAppId = referralInfo != null ? referralInfo.ReferredByAppId ?? null : null,
                    TotalReferred = numTotal,
                    TotalReferredActive = numActive,
                    IsActive = referralInfo != null && (dateNow - referralInfo.TimeStamp) < TimeSpan.FromDays(6 * 30),
                });
            }
        }

        /// <summary>
        /// If not signed up for a referral, you can add another user as a referral.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/user/giftreferral")]
        public async Task<IHttpActionResult> GiftReferral(UserRefGiftRequest req)
        {
            if (req == null || String.IsNullOrEmpty(req.UserAppId)) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (user.ReferralInfo != null)
                {
                    return BadRequest();
                }

                user.ReferralInfo = new Models.Database.Referral()
                {
                    ReferredByAppId = req.UserAppId,
                    TimeStamp = DateTime.UtcNow,
                };

                await db.SaveChangesAsync();

                return Ok(new UserRefGiftResponse() { success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/referralcode")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetReferralCode()
        {
            var userAppId = User.Identity.GetUserId();
            if (userAppId == null)
            {
                return Unauthorized();
            }

            using (var db = new ZapContext())
            {
                var refCode = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Select(u => u.ReferralCode)
                    .AsNoTracking()
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                if (refCode == null)
                {
                    var user = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync().ConfigureAwait(true);
                    if (user == null) return NotFound();
                    user.ReferralCode = CryptoService.GetNewRefCode();
                    refCode = user.ReferralCode;
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }

                var numrefs = await db.Referrals
                    .Where(r => r.ReferredByAppId == userAppId)
                    .CountAsync();

                return Ok(new GetRefCodeResponse()
                {
                    refCode = refCode
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("api/v1/user/email")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetUserEmail()
        {
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var userInfo = await userManager.FindByIdAsync(userAppId);
                
                if (userInfo == null) return NotFound();

                string userEmail = userInfo.Email;
                    
                if (userEmail == null) return NotFound();

                return Ok(new GetUserEmailResponse()
                {
                    Email = userEmail,
                    success = true
                });
            }
        }

        private async Task<double> GetUserBalance()
        {
            double balance = 0.0;
            var userAppId = User.Identity.GetUserId();            // Get the logged in user ID

            if (userAppId == null)
            {
                return 0.0;
            }

            try
            {
                using (var db = new ZapContext())
                {
                    var userBalance = await db.Users
                        .Where(u => u.AppId == userAppId)
                        .Select(u => new
                        {
                            Value = u.Funds == null ? -1 : u.Funds.Balance
                        })
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    if (userBalance == null)
                    {
                        // User not found in database, or not logged in
                        return 0.0;
                    }
                    else
                    {
                        balance = userBalance.Value;
                    }
                }
            }
            catch (Exception e)
            {
                BackgroundJob.Enqueue<MailingService>(x => x.SendI(
                    new UserEmailModel()
                    {
                        Destination = System.Configuration.ConfigurationManager.AppSettings["ExceptionReportEmail"],
                        Body = " Exception: " + e.Message + "\r\n Stack: " + e.StackTrace + "\r\n method: UserBalance" + "\r\n user: " + userAppId,
                        Email = "",
                        Name = "zapread.com Exception",
                        Subject = "User ApiController error",
                    }, "Accounts", true));

                // If we have an exception, it is possible a user is trying to abuse the system.  Return 0 to be uninformative.
                balance = 0.0;
            }

            return Math.Floor(balance);
        }
    }
}