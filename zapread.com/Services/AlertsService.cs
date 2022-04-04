using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Helpers;
using zapread.com.Models.Database;

namespace zapread.com.Services
{
    /// <summary>
    /// Manage alerts which are sent to users (as a background process)
    /// 
    /// Uses Hangfire for async and scalable handling
    /// </summary>
    public class AlertsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool AlertPostComment(long commentId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments
                    .Where(c => c.CommentId == commentId)
                    .Select(c => new 
                    {
                        c.Post.PostId,
                        c.Post.PostTitle,
                        CommentUserName = c.UserId.Name
                    }).FirstOrDefault();

                if (commentInfo == null)
                {
                    return false;
                }

                var postInfo = db.Posts
                    .Where(p => p.PostId == commentInfo.PostId)
                    .Select(p => new
                    {
                        AlertOnOwnPostCommented = p.UserId.Settings != null && p.UserId.Settings.AlertOnOwnPostCommented,
                        FollowerAppIds = p.FollowedByUsers.Select(u => u.AppId)
                    })
                    .FirstOrDefault();
                
                // send alerts to post owner
                if (postInfo != null && postInfo.AlertOnOwnPostCommented)
                {
                    var postOwner = db.Posts
                        .Where(p => p.PostId == commentInfo.PostId)
                        .Select(p => p.UserId)
                        .FirstOrDefault();

                    // TODO: change the CommentLink and PostLink to reference just the ids instead of full objects
                    UserAlert alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New comment on your post: <a href=" +
                            "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                                commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                            ">" + commentInfo.PostTitle + "</a>",
                        Content = "From: <a href='" +
                            "/user/" + HttpUtility.UrlEncode(commentInfo.CommentUserName) +
                            "'>" + commentInfo.CommentUserName + "</a>",
                        CommentLink = db.Comments.FirstOrDefault(c => c.CommentId == commentId),
                        IsDeleted = false,
                        IsRead = false,
                        To = postOwner,
                        PostLink = db.Posts.FirstOrDefault(p => p.PostId == commentInfo.PostId),
                    };
                    postOwner.Alerts.Add(alert);
                    db.SaveChanges();
                }

                if (postInfo != null && postInfo.FollowerAppIds.Any())
                {
                    foreach (var followerAppId in postInfo.FollowerAppIds)
                    {
                        var postFollower = db.Users
                            .Where(u => u.AppId == followerAppId)
                            .FirstOrDefault();

                        if (postFollower != null)
                        {
                            // TODO: change the CommentLink and PostLink to reference just the ids instead of full objects
                            UserAlert alert = new UserAlert()
                            {
                                TimeStamp = DateTime.Now,
                                Title = "New comment on a post you are following: <a href=" +
                                    "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                                        commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                                    ">" + commentInfo.PostTitle + "</a>",
                                Content = "From: <a href='" +
                                    "/user/" + HttpUtility.UrlEncode(commentInfo.CommentUserName) +
                                    "'>" + commentInfo.CommentUserName + "</a>",
                                CommentLink = db.Comments.FirstOrDefault(c => c.CommentId == commentId),
                                IsDeleted = false,
                                IsRead = false,
                                To = postFollower,
                                PostLink = db.Posts.FirstOrDefault(p => p.PostId == commentInfo.PostId),
                            };
                            postFollower.Alerts.Add(alert);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return true;
        }
    }
}

