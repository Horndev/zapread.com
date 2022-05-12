using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models;
using zapread.com.Models.API;
using zapread.com.Models.API.Post;
using zapread.com.Models.Database;

namespace zapread.com.API
{
    /// <summary>
    /// Controller for Post API
    /// </summary>
    public class PostController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/reactions/add")]
        [AcceptVerbs("POST")]
        [ValidateJsonAntiForgeryToken]
        public async Task<IHttpActionResult> AddReaction(AddReactionRequest req)
        {
            if (req == null) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var userPostReactions = db.Posts
                   .Where(p => p.PostId == req.PostId)
                   .SelectMany(p => p.PostReactions.Where(r => r.User.AppId == userAppId))
                   .Select(r => r.Reaction.ReactionId);

                var alreadyReacted = await db.Posts
                    .Where(p => p.PostId == req.PostId)
                    .Select(p => p.PostReactions.Any(r => r.Reaction.ReactionId == req.ReactionId && r.User.AppId == userAppId))
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                List<ReactionItem> reactions = new List<ReactionItem>();

                if (alreadyReacted)
                {
                    var postToRemoveReaction = await db.Posts
                        .Where(p => p.PostId == req.PostId)
                        .Include(p => p.PostReactions)
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    var reactionToRemove = await db.Posts
                        .Where(p => p.PostId == req.PostId)
                        .Select(p => p.PostReactions
                            .Where(r => r.Reaction.ReactionId == req.ReactionId && r.User.AppId == userAppId)
                            .FirstOrDefault())
                        .FirstOrDefaultAsync().ConfigureAwait(true);

                    postToRemoveReaction.PostReactions.Remove(reactionToRemove);

                    await db.SaveChangesAsync();

                    reactions = await db.Posts
                        .Where(p => p.PostId == req.PostId)
                        .SelectMany(p => p.PostReactions)
                        .GroupBy(r => r.Reaction)
                        .Select(g => new ReactionItem()
                        {
                            ReactionId = g.Key.ReactionId,
                            ReactionIcon = g.Key.ReactionIcon,
                            NumReactions = g.Count(),
                            UserNames = g.Select(r => r.User.Name).Take(5).ToList(),
                            IsApplied = userPostReactions.Contains(g.Key.ReactionId)
                        })
                        .ToListAsync().ConfigureAwait(false);

                    return Ok(new AddReactionResponse()
                    {
                        Reactions = reactions,
                        AlreadyReacted = true,
                        success = true,
                    });
                }

                var post = await db.Posts
                    .Where(p => p.PostId == req.PostId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var user = await db.Users
                    .Where(u => u.AppId == userAppId)
                    .Where(u => u.AvailableReactions
                        .Union(db.Reactions.Where(r => r.UnlockedAll))
                        .Select(r => r.ReactionId).Contains(req.ReactionId))
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                // If user hasn't unlocked reaction
                if (user == null)
                {
                    return Ok(new AddReactionResponse()
                    {
                        NotAvailable = true,
                        AlreadyReacted = false,
                        success = false,
                    });
                }

                var reaction = await db.Reactions
                    .Where(r => r.ReactionId == req.ReactionId)
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var postReaction = new PostReaction()
                {
                    Reaction = reaction,
                    User = user,
                    TimeStamp = DateTime.UtcNow,
                };

                post.PostReactions.Add(postReaction);

                await db.SaveChangesAsync();

                reactions = await db.Posts
                    .Where(p => p.PostId == req.PostId)
                    .SelectMany(p => p.PostReactions)
                    .GroupBy(r => r.Reaction)
                    .Select(g => new ReactionItem()
                    {
                        ReactionId = g.Key.ReactionId,
                        ReactionIcon = g.Key.ReactionIcon,
                        NumReactions = g.Count(),
                        UserNames = g.Select(r => r.User.Name).Take(5).ToList(),
                        IsApplied = userPostReactions.Contains(g.Key.ReactionId)
                    })
                    .ToListAsync().ConfigureAwait(false);

                return Ok(new AddReactionResponse()
                {
                    Reactions = reactions,
                    AlreadyReacted = false,
                    success = true,
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [Route("api/v1/post/reactions/list/{postId}")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetReactions(int postId)
        {
            if (postId < 1) return BadRequest();
            var userAppId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var userPostReactions = db.Posts
                    .Where(p => p.PostId == postId)
                    .SelectMany(p => p.PostReactions.Where(r => r.User.AppId == userAppId))
                    .Select(r => r.Reaction.ReactionId);

                var reactions = await db.Posts
                    .Where(p => p.PostId == postId)
                    .SelectMany(p => p.PostReactions)
                    .GroupBy(r => r.Reaction)
                    .Select(g => new ReactionItem()
                    {
                        ReactionId = g.Key.ReactionId,
                        ReactionIcon = g.Key.ReactionIcon,
                        NumReactions = g.Count(),
                        UserNames = g.Select(r => r.User.Name).Take(5).ToList(),
                        IsApplied = userPostReactions.Contains(g.Key.ReactionId)
                    })
                    .ToListAsync().ConfigureAwait(false);

                return Ok(new GetReactionsResponse()
                {
                    Reactions = reactions,
                    success = true
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/unignore")]
        [AcceptVerbs("POST")]
        [ValidateJsonAntiForgeryToken]
        public async Task<IHttpActionResult> UnIgnore(PostReqParameters req)
        {
            if (req == null || req.PostId < 1) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.IgnoringPosts)
                    .FirstOrDefaultAsync(u => u.AppId == userAppId)
                    .ConfigureAwait(true); // keep context

                var post = await db.Posts
                    .FirstOrDefaultAsync(p => p.PostId == req.PostId)
                    .ConfigureAwait(true); // keep context 

                if (user.IgnoringPosts.Contains(post))
                {
                    user.IgnoringPosts.Remove(post);
                    await db.SaveChangesAsync();
                }

                return Ok(new ZapReadResponse() { success = true });
            }
        }

        /// <summary>
        /// Follow a post
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/ignore")]
        [AcceptVerbs("POST")]
        [ValidateJsonAntiForgeryToken]
        public async Task<IHttpActionResult> Ignore(PostReqParameters req)
        {
            if (req == null || req.PostId < 1) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.IgnoringPosts)
                    .FirstOrDefaultAsync(u => u.AppId == userAppId)
                    .ConfigureAwait(true); // keep context

                var post = await db.Posts
                    .FirstOrDefaultAsync(p => p.PostId == req.PostId)
                    .ConfigureAwait(true); // keep context 

                if (!user.IgnoringPosts.Contains(post))
                {
                    user.IgnoringPosts.Add(post);
                    await db.SaveChangesAsync();
                }

                return Ok(new ZapReadResponse() { success = true });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/unfollow")]
        [AcceptVerbs("POST")]
        [ValidateJsonAntiForgeryToken]
        public async Task<IHttpActionResult> UnFollow(PostReqParameters req)
        {
            if (req == null || req.PostId < 1) return BadRequest();

            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.FollowingPosts)
                    .FirstOrDefaultAsync(u => u.AppId == userAppId)
                    .ConfigureAwait(true); // keep context

                var post = await db.Posts
                    .FirstOrDefaultAsync(p => p.PostId == req.PostId)
                    .ConfigureAwait(true); // keep context 

                if (user.FollowingPosts.Contains(post))
                {
                    user.FollowingPosts.Remove(post);
                    await db.SaveChangesAsync();
                }

                return Ok(new ZapReadResponse() { success = true });
            }
        }

        /// <summary>
        /// Follow a post
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/follow")]
        [AcceptVerbs("POST")]
        [ValidateJsonAntiForgeryToken]
        public async Task<IHttpActionResult> Follow(PostReqParameters req)
        {
            if (req == null || req.PostId < 1) return BadRequest();
            
            var userAppId = User.Identity.GetUserId();

            if (userAppId == null) return Unauthorized();

            using (var db = new ZapContext())
            {
                var user = await db.Users
                    .Include(u => u.FollowingPosts)
                    .FirstOrDefaultAsync(u => u.AppId == userAppId)
                    .ConfigureAwait(true); // keep context

                var post = await db.Posts
                    .FirstOrDefaultAsync(p => p.PostId == req.PostId)
                    .ConfigureAwait(true); // keep context 

                if (!user.FollowingPosts.Contains(post))
                {
                    user.FollowingPosts.Add(post);
                    await db.SaveChangesAsync();
                }

                return Ok(new ZapReadResponse() { success = true });
            }
        }

        /// <summary>
        /// Get information about a post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [Route("api/v1/post/getinfo/{postId}")]
        [AcceptVerbs("GET")]
        public ZapReadResponse GetInfo(int postId)
        {
            return new ZapReadResponse() { success = false, message = Properties.Resources.ErrorNotImplemented };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Route("api/v1/post/comments/loadmore")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> GetMoreCommentsResponse(GetMoreCommentsParameters req)
        {
            if (req == null)
            {
                return BadRequest();
            }

            if (req.Rootshown == null)
            {
                return BadRequest();
            }

            var userAppId = User.Identity.GetUserId();

            using (var db = new ZapContext())
            {
                var shown = String.IsNullOrEmpty(req.Rootshown) ? new List<long>() : req.Rootshown.Split(';').Select(s => Convert.ToInt64(s, CultureInfo.InvariantCulture)).ToList();

                IQueryable<Comment> commentsq;
                if (req.ParentCommentId < 0)
                {
                    commentsq = db.Posts
                        .Where(p => p.PostId == req.PostId)
                        .SelectMany(p => p.Comments)
                        .Where(c => !shown.Contains(c.CommentId))
                        .Where(c => !c.IsReply)
                        .OrderByDescending(c => c.Score)
                        .ThenBy(c => c.TimeStamp)
                        .Take(3); // This is only for root comments!
                } 
                else
                {
                    commentsq = db.Comments
                        .Where(c => c.CommentId == req.ParentCommentId)
                        .SelectMany(c => c.Replies)
                        .Where(c => !shown.Contains(c.CommentId))
                        .OrderByDescending(c => c.Score)
                        .ThenBy(c => c.TimeStamp);
                }

                var comments = await commentsq
                    .SelectMany(rootComment =>
                        rootComment.Replies
                            .OrderByDescending(c1 => c1.Score)
                            .ThenBy(c1 => c1.TimeStamp)
                            .Take(3)
                            .Union(rootComment.Replies
                                .SelectMany(rcreplies => rcreplies.Replies
                                    .OrderByDescending(c1 => c1.Score)
                                    .ThenBy(c1 => c1.TimeStamp)
                                    .Take(3)
                                )
                            )
                            .Union(new List<Comment>() { rootComment })
                    ) // Return replies 3 layers deep
                    .Select(c => new PostCommentsViewModel()
                    {
                        PostId = req.PostId,
                        CommentId = c.CommentId,
                        Text = c.Text,
                        Score = c.Score,
                        IsReply = c.IsReply,
                        NumReplies = c.Replies.Count(),
                        IsDeleted = c.IsDeleted,
                        TimeStamp = c.TimeStamp,
                        TimeStampEdited = c.TimeStampEdited,
                        UserId = c.UserId.Id,
                        UserName = c.UserId.Name,
                        UserAppId = c.UserId.AppId,
                        ProfileImageVersion = c.UserId.ProfileImage.Version,
                        ViewerUpvoted = userAppId == null ? false : c.VotesUp.Select(v => v.AppId).Contains(userAppId),
                        ViewerDownvoted = userAppId == null ? false : c.VotesDown.Select(v => v.AppId).Contains(userAppId),
                        ViewerIgnoredUser = userAppId == null ? false : c.UserId.AppId == userAppId ? false : c.UserId.IgnoredByUsers.Select(u => u.AppId).Contains(userAppId),
                        ParentCommentId = c.Parent == null ? 0 : c.Parent.CommentId,
                        ParentUserId = c.Parent == null ? 0 : c.Parent.UserId.Id,
                        ParentUserAppId = c.Parent == null ? "" : c.Parent.UserId.AppId,
                        ParentUserName = c.Parent == null ? "" : c.Parent.UserId.Name,
                    })
                    .ToListAsync().ConfigureAwait(false);

                shown.AddRange(comments.Select(c => c.CommentId));

                return Ok(new GetMoreCommentsResponse()
                {
                    success = true,
                    Shown = String.Join(";", shown),
                    Comments = comments,
                    HasMoreComments = await commentsq.CountAsync() > 3,
                });
            }
        }
    }
}