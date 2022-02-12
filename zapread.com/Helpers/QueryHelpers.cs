using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;
using zapread.com.Models;
using zapread.com.Models.Database;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class QueryHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        public class PostQueryUserInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public int Id;

            /// <summary>
            /// 
            /// </summary>
            public string AppId;

            /// <summary>
            /// 
            /// </summary>
            public bool ViewAllLanguages;

            /// <summary>
            /// 
            /// </summary>
            public List<int> IgnoredGroups;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLanguages"></param>
        /// <param name="db"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static IQueryable<Post> QueryValidPosts(List<string> userLanguages, ZapContext db, PostQueryUserInfo userInfo = null)
        {
            IQueryable<Post> validposts = db.Posts
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft);

            if (userInfo != null)
            {
                var ig = userInfo.IgnoredGroups;
                validposts = validposts
                    .Where(p => !ig.Contains(p.Group.GroupId));

                var allLang = userInfo.ViewAllLanguages;
                if (!allLang)
                {
                    var languages = userLanguages ?? new List<string>() { "en" };
                    validposts = validposts
                        .Where(p => p.Language == null || languages.Contains(p.Language));
                }
            }
            else
            {
                if (userLanguages == null)
                {
                    validposts = db.Posts
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft);
                }
                else
                {
                    validposts = db.Posts
                        .Where(p => !p.IsDeleted)
                        .Where(p => !p.IsDraft)
                        .Where(p => p.Language == null || userLanguages.Contains(p.Language));
                }
            }

            return validposts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<PostViewModel>> QueryActivityPostsVm(int start, int count, int userId = 0)
        {
            using (var db = new ZapContext())
            {
                //List<int> followingIds = await db.Users
                //        .Where(us => us.Id == userId)
                //        .SelectMany(us => us.Following)
                //        .Select(f => f.Id)
                //        .ToListAsync()
                //        .ConfigureAwait(true);

                var userposts = db.Posts
                    .Where(p => p.UserId.Id == userId)
                    .Where(p => !p.IsDeleted)
                    .Where(p => !p.IsDraft)
                    .OrderByDescending(p => p.TimeStamp)
                    //.Take(20)
                    .Select(p => new PostViewModel()
                    {
                        PostTitle = p.PostTitle,
                        Content = p.Content,
                        PostId = p.PostId,
                        GroupId = p.Group.GroupId,
                        GroupName = p.Group.GroupName,
                        IsSticky = p.IsSticky,
                        UserName = p.UserId.Name,
                        UserId = p.UserId.Id,
                        UserAppId = p.UserId.AppId,
                        UserProfileImageVersion = p.UserId.ProfileImage.Version,
                        Score = p.Score,
                        TimeStamp = p.TimeStamp,
                        TimeStampEdited = p.TimeStampEdited,
                        IsNSFW = p.IsNSFW,
                        ViewerIsMod = p.Group.Moderators.Select(m => m.Id).Contains(userId),
                        ViewerUpvoted = p.VotesUp.Select(v => v.Id).Contains(userId),
                        ViewerDownvoted = p.VotesDown.Select(v => v.Id).Contains(userId),
                        ViewerIgnoredUser = p.UserId.Id == userId ? false : p.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                        CommentVms = p.Comments.Select(c => new PostCommentsViewModel()
                        {
                            PostId = p.PostId,
                            CommentId = c.CommentId,
                            Text = c.Text,
                            Score = c.Score,
                            IsReply = c.IsReply,
                            IsDeleted = c.IsDeleted,
                            TimeStamp = c.TimeStamp,
                            TimeStampEdited = c.TimeStampEdited,
                            UserId = c.UserId.Id,
                            UserName = c.UserId.Name,
                            UserAppId = c.UserId.AppId,
                            ProfileImageVersion = c.UserId.ProfileImage.Version,
                            ViewerUpvoted = c.VotesUp.Select(v => v.Id).Contains(userId),
                            ViewerDownvoted = c.VotesDown.Select(v => v.Id).Contains(userId),
                            ViewerIgnoredUser = c.UserId.Id == userId ? false : c.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                            ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                            ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                            ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                            ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                        }),
                    });

                // Posts by users who are following this user
                //var followposts = db.Posts
                //    .Where(p => followingIds.Contains(p.UserId.Id))
                //    .Where(p => !p.IsDeleted)
                //    .Where(p => !p.IsDraft)
                //    .OrderByDescending(p => p.TimeStamp)
                //    .Take(20)
                //    .Select(p => new PostViewModel()
                //    {
                //        PostTitle = p.PostTitle,
                //        Content = p.Content,
                //        PostId = p.PostId,
                //        GroupId = p.Group.GroupId,
                //        GroupName = p.Group.GroupName,
                //        IsSticky = p.IsSticky,
                //        UserName = p.UserId.Name,
                //        UserId = p.UserId.Id,
                //        UserAppId = p.UserId.AppId,
                //        UserProfileImageVersion = p.UserId.ProfileImage.Version,
                //        Score = p.Score,
                //        TimeStamp = p.TimeStamp,
                //        TimeStampEdited = p.TimeStampEdited,
                //        IsNSFW = p.IsNSFW,
                //        ViewerIsMod = p.Group.Moderators.Select(m => m.Id).Contains(userId),
                //        ViewerUpvoted = p.VotesUp.Select(v => v.Id).Contains(userId),
                //        ViewerDownvoted = p.VotesDown.Select(v => v.Id).Contains(userId),
                //        ViewerIgnoredUser = p.UserId.Id == userId ? false : p.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                //        CommentVms = p.Comments.Select(c => new PostCommentsViewModel()
                //        {
                //            PostId = p.PostId,
                //            CommentId = c.CommentId,
                //            Text = c.Text,
                //            Score = c.Score,
                //            IsReply = c.IsReply,
                //            IsDeleted = c.IsDeleted,
                //            TimeStamp = c.TimeStamp,
                //            TimeStampEdited = c.TimeStampEdited,
                //            UserId = c.UserId.Id,
                //            UserName = c.UserId.Name,
                //            UserAppId = c.UserId.AppId,
                //            ProfileImageVersion = c.UserId.ProfileImage.Version,
                //            ViewerUpvoted = c.VotesUp.Select(v => v.Id).Contains(userId),
                //            ViewerDownvoted = c.VotesDown.Select(v => v.Id).Contains(userId),
                //            ViewerIgnoredUser = c.UserId.Id == userId ? false : c.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                //            ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                //            ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                //            ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                //        }),
                //    });
                //var activityposts = userposts.ToList().Union(followposts.ToList())
                //    .OrderByDescending(p => p.TimeStamp)
                //    .Skip(start)
                //    .Take(count)
                //    .ToList();

                var activityposts = await userposts
                    .Skip(start)
                    .Take(count)
                    .ToListAsync().ConfigureAwait(true);

                return activityposts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validposts"></param>
        /// <returns></returns>
        public static IQueryable<PostQueryInfo> OrderPostsByActive(IQueryable<Post> validposts)
        {
            DateTime scoreStart = new DateTime(2018, 07, 01);
            var sposts = validposts
                .Where(p => !p.IsDeleted)
                .Where(p => !p.IsDraft)
                .Select(p => new
                {
                    p,
                    s = (Math.Abs((double)p.Comments.Count) < 1.0 ? 1.0 : 100000.0 * Math.Abs((double)p.Comments.Count)),    // Max (|x|,1)                                                           
                })
                .Select(p => new
                {
                    p.p,
                    order = SqlFunctions.Log10(p.s),
                    sign = p.p.Comments.Count >= 0.0 ? 1.0 : -1.0,                              // Sign of s
                    dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                })
                .Select(p => new PostQueryInfo()
                {
                    p = p.p,
                    hot = 0,
                    active = (p.sign * p.order) + (p.dt / 2000000.0) // Reduced time effect
                })
                .OrderByDescending(p => p.active);
                //.Select(p => p.p);

            return sposts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validposts"></param>
        /// <param name="groupId"></param>
        /// <param name="stickyPostOnTop"></param>
        /// <returns></returns>
        public static IQueryable<PostQueryInfo> OrderPostsByNew(IQueryable<Post> validposts, int groupId = 0, bool stickyPostOnTop = false)
        {
            IQueryable<PostQueryInfo> sposts = validposts
                .Where(p => p.Score > -50)
                .Select(p => new PostQueryInfo() {
                p = p,
                hot = 0,
            });

            // filter by group
            if (groupId > 0)
            {
                sposts = sposts.Where(p => p.p.Group.GroupId == groupId);
            }

            if (stickyPostOnTop)
            {
                sposts = sposts.OrderByDescending(p => new { p.p.IsSticky, p.p.TimeStamp });
            }
            else
            {
                sposts = sposts.OrderByDescending(p => p.p.TimeStamp);
            }

            return sposts;
        }

        /// <summary>
        /// 
        /// </summary>
        public class PostQueryInfo
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public Post p;
            public double? hot;
            public double? order1;
            public double? order2;
            public double? sign;
            public double? dt;
            public double? active;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validposts"></param>
        /// <returns></returns>
        public static IQueryable<PostQueryInfo> OrderPostsByScore(IQueryable<Post> validposts)
        {
            DateTime scoreStart = DateTime.Now - TimeSpan.FromDays(30); //new DateTime(2018, 07, 01);
            var sposts = validposts
                .Where(p => !p.IsDeleted)
                .Where(p => !p.IsDraft)
                .Select(p => new
                {
                    p,
                    // Includes the sum of absolute value of comment scores
                    cScore = p.Comments.Any() ? p.Comments.Where(c => !c.IsDeleted).Sum(c => Math.Abs((double)c.Score) < 1.0 ? 1.0 : Math.Abs((double)c.Score)) : 1.0
                })
                .Select(p => new
                {
                    p.p,
                    p.cScore,
                    s = (Math.Abs((double)p.p.Score) < 1.0 ? 1.0 : Math.Abs((double)p.p.Score)),    // Max (|x|,1)                                                           
                })
                .Select(p => new
                {
                    p.p,
                    order1 = SqlFunctions.Log10(p.s),
                    order2 = SqlFunctions.Log10(p.cScore < 1.0 ? 1.0 : p.cScore),     // Comment scores
                    sign = p.p.Score > 0.0 ? 1.0 : -1.0,                              // Sign of s
                    dt = 1.0 * DbFunctions.DiffSeconds(scoreStart, p.p.TimeStamp),    // time since start
                })
                .Select(p => new PostQueryInfo()
                {
                    p = p.p,
                    order1 = p.order1,
                    order2 = p.order2,
                    sign = p.sign,
                    dt = p.dt,
                    hot = (p.sign * (p.order1 + p.order2)) + (p.dt / (p.sign > 0 ? 90000.0 : 900000.0))
                })
                .OrderByDescending(p => p.hot);
            //.Select(p => p.p);

            return sposts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="postquery"></param>
        /// <param name="userInfo"></param>
        /// <param name="userId"></param>
        /// <param name="numComments"></param>
        /// <returns></returns>
        public static async Task<List<PostViewModel>> QueryPostsVm(int start, int count, IQueryable<PostQueryInfo> postquery, PostQueryUserInfo userInfo = null, int numComments = 3)
        {
            int userId = 0;

            if (userInfo != null)
            {
                userId = userInfo.Id;
            }

            var sposts = await postquery
                .Skip(start)
                .Take(count)
                .Select(p => new PostViewModel()
                {
                    Hot = p.hot??0,
                    PostTitle = p.p.PostTitle,
                    Content = p.p.Content,
                    PostId = p.p.PostId,
                    GroupId = p.p.Group.GroupId,
                    GroupName = p.p.Group.GroupName,
                    IsSticky = p.p.IsSticky,
                    UserName = p.p.UserId.Name,
                    UserId = p.p.UserId.Id,
                    UserAppId = p.p.UserId.AppId,
                    UserProfileImageVersion = p.p.UserId.ProfileImage.Version,
                    Score = p.p.Score,
                    TimeStamp = p.p.TimeStamp,
                    TimeStampEdited = p.p.TimeStampEdited,
                    IsNSFW = p.p.IsNSFW,
                    ViewerIsMod = p.p.Group.Moderators.Select(m => m.Id).Contains(userId),
                    ViewerUpvoted = p.p.VotesUp.Select(v => v.Id).Contains(userId),
                    ViewerDownvoted = p.p.VotesDown.Select(v => v.Id).Contains(userId),
                    ViewerIgnoredUser = p.p.UserId.Id == userId ? false : p.p.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                    CommentVms = p.p.Comments.Select(c => new PostCommentsViewModel()
                    {
                        PostId = p.p.PostId,
                        CommentId = c.CommentId,
                        Text = c.Text,
                        Score = c.Score,
                        IsReply = c.IsReply,
                        IsDeleted = c.IsDeleted,
                        TimeStamp = c.TimeStamp,
                        TimeStampEdited = c.TimeStampEdited,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        ViewerUpvoted = c.VotesUp.Select(v => v.Id).Contains(userId),
                        ViewerDownvoted = c.VotesDown.Select(v => v.Id).Contains(userId),
                        ViewerIgnoredUser = c.UserId.Id == userId ? false : c.UserId.IgnoredByUsers.Select(u => u.Id).Contains(userId),
                        ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                        ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                        ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                        ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                    }),
                })
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(true);

            return sposts;
        }
    }
}