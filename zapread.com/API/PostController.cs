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