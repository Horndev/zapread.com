using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool AlertGroupModGranted(int groupId, int userId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var groupInfo = db.Groups
                    .Where(g => g.GroupId == groupId)
                    .Select(g => new
                    {
                        g.GroupName
                    })
                    .FirstOrDefault();

                var userInfo = db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        User = u
                    })
                    .FirstOrDefault();

                if (groupInfo != null && userInfo != null)
                {
                    UserAlert alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "Group moderation granted",
                        Content = "You have been granted moderation priviliages in " +
                            "<a href='/Group/Detail/" + Convert.ToString(groupId) + "/'>" + groupInfo.GroupName + "</a>.",
                        CommentLink = null,
                        IsDeleted = false,
                        IsRead = false,
                        To = userInfo.User,
                        PostLink = null,
                    };

                    userInfo.User.Alerts.Add(alert);
                    db.SaveChanges();
                }

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool AlertGroupAdminGranted(int groupId, int userId, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var groupInfo = db.Groups
                    .Where(g => g.GroupId == groupId)
                    .Select(g => new
                    {
                        g.GroupName
                    })
                    .FirstOrDefault();

                var userInfo = db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        User = u
                    })
                    .FirstOrDefault();

                if (groupInfo != null && userInfo != null)
                {
                    UserAlert alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "Group administration granted",
                        Content = "You have been granted administration priviliages in " +
                            "<a href='/Group/Detail/" + Convert.ToString(groupId) + "/'>" + groupInfo.GroupName + "</a>.",
                        CommentLink = null,
                        IsDeleted = false,
                        IsRead = false,
                        To = userInfo.User,
                        PostLink = null,
                    };

                    userInfo.User.Alerts.Add(alert);
                    db.SaveChanges();
                }

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdFollowed"></param>
        /// <param name="userIdFollowing"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool AlertUserNewFollower(int userIdFollowed, int userIdFollowing, bool isTest = false)
        {
            using (var db = new ZapContext())
            {
                var userInfo = db.Users
                    .Where(u => u.Id == userIdFollowed)
                    .Select(u => new
                    {
                        u.AppId,
                        User = u
                    })
                    .FirstOrDefault();

                var otherUserInfo = db.Users
                    .Where(u => u.Id == userIdFollowing)
                    .Select(u => new
                    {
                        u.AppId,
                        u.Name
                    })
                    .FirstOrDefault();

                if (userInfo != null && otherUserInfo != null)
                {
                    UserAlert alert = new UserAlert()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "<a href=" +
                            "/user/" + HttpUtility.UrlEncode(otherUserInfo.Name.Trim()) + "/" +
                            ">" + otherUserInfo.Name + "</a> " +
                            "is now following you!",
                        Content = "",
                        CommentLink = null,
                        IsDeleted = false,
                        IsRead = false,
                        To = userInfo.User,
                        PostLink = null,
                    };

                    userInfo.User.Alerts.Add(alert);
                    db.SaveChanges();
                }

                return true;
            }
        }

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
                        c.Text,
                        c.Post.PostId,
                        c.Post.PostTitle,
                        To = c.Post.UserId,
                        From = c.UserId,
                        c.Post,
                        Comment = c,
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
                    //UserAlert alert = new UserAlert()
                    //{
                    //    TimeStamp = DateTime.Now,
                    //    Title = "New comment on your post: <a href=" +
                    //        "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                    //            commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                    //        ">" + commentInfo.PostTitle + "</a>",
                    //    Content = "From: <a href='" +
                    //        "/user/" + HttpUtility.UrlEncode(commentInfo.CommentUserName) +
                    //        "'>" + commentInfo.CommentUserName + "</a>",
                    //    CommentLink = db.Comments.FirstOrDefault(c => c.CommentId == commentId),
                    //    IsDeleted = false,
                    //    IsRead = false,
                    //    To = postOwner,
                    //    PostLink = db.Posts.FirstOrDefault(p => p.PostId == commentInfo.PostId),
                    //};
                    //postOwner.Alerts.Add(alert);

                    UserMessage message = new UserMessage()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New comment on your post: "
                            + "<a href='" +
                            "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                                commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                            "'>" + commentInfo.PostTitle + "</a>"
                            + (commentInfo.PostTitle != null ? commentInfo.PostTitle : "Post") + "</a>",
                        Content = commentInfo.Text,
                        CommentLink = commentInfo.Comment,
                        IsDeleted = false,
                        IsRead = false,
                        To = commentInfo.To,
                        PostLink = commentInfo.Post,
                        From = commentInfo.From,
                    };

                    postOwner.Messages.Add(message);

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
                                Title = "New comment on a post you are following: <a href='" +
                                    "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                                        commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                                    "'>" + commentInfo.PostTitle + "</a>",
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public bool AlertPostCommentReply(long commentId)
        {
            using (var db = new ZapContext())
            {
                var commentInfo = db.Comments
                    .Where(c => c.CommentId == commentId)
                    .Select(c => new {
                        c.Parent.UserId.Settings.AlertOnOwnCommentReplied,
                        c.CommentId,
                        CommentUserName = c.UserId.Name,
                        To = c.Parent.UserId,
                        From = c.UserId,
                        c.Post,
                        c.Post.PostId,
                        c.Post.PostTitle,
                        c.Text,
                        Comment = c
                    })
                    .FirstOrDefault();

                if (commentInfo != null && commentInfo.AlertOnOwnCommentReplied)
                {
                    //UserAlert alert = new UserAlert()
                    //{
                    //    TimeStamp = DateTime.Now,
                    //    Title = "New reply to <a href='" +
                    //        "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                    //            commentInfo.PostTitle?.MakeURLFriendly() + "/" + "#cid_" + Convert.ToString(commentInfo.CommentId) +
                    //        "'>" + "your comment" + "</a>",
                    //    Content = "From: <a href='" +
                    //        "/user/" + HttpUtility.UrlEncode(commentInfo.CommentUserName) +
                    //        "'>" + commentInfo.CommentUserName + "</a>",
                    //    CommentLink = commentInfo.Comment,
                    //    IsDeleted = false,
                    //    IsRead = false,
                    //    To = commentInfo.To,
                    //    PostLink = commentInfo.Post,
                    //};
                    //commentInfo.To.Alerts.Add(alert);

                    UserMessage message = new UserMessage()
                    {
                        TimeStamp = DateTime.Now,
                        Title = "New reply to your comment in post: "
                            + "<a href='" +
                                "/p/" + CryptoService.IntIdToString(commentInfo.PostId) + "/" +
                                    commentInfo.PostTitle?.MakeURLFriendly() + "/" +
                                "'>" + commentInfo.PostTitle + "</a>"
                            + (commentInfo.PostTitle != null ? commentInfo.PostTitle : "Post") + "</a>",
                        Content = commentInfo.Text,
                        CommentLink = commentInfo.Comment,
                        IsDeleted = false,
                        IsRead = false,
                        To = commentInfo.To,
                        PostLink = commentInfo.Post,
                        From = commentInfo.From,
                    };

                    commentInfo.To.Messages.Add(message);

                    db.SaveChanges();
                }

                return true;
            }
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public bool AlertNewPost(long postId)
        {
            using (var db = new ZapContext())
            {
                var followUsers = db.Posts
                    .Where(p => p.PostId == postId)
                    .SelectMany(p => p.UserId.Followers)
                    .Select(u => new
                    {
                        u.Id,
                        u.AppId,
                        u.Settings.NotifyOnNewPostSubscribedUser,
                        u.Settings.AlertOnNewPostSubscribedUser,
                        user = u,
                    }).ToList();

                if (followUsers.Any())
                {
                    var postInfo = db.Posts
                        .Where(p => p.PostId == postId)
                        .Select(p => new
                        {
                            Post = p,
                            p.UserId.Name
                        })
                        .FirstOrDefault();

                    foreach (var follower in followUsers)
                    {
                        // Add Alert
                        if (follower.AlertOnNewPostSubscribedUser)
                        {
                            var alert = new UserAlert()
                            {
                                TimeStamp = DateTime.Now,
                                Title = "New post by a user you are following: <a href='/User/" + Uri.EscapeDataString(postInfo.Name) + "'>" + postInfo.Name + "</a>",
                                Content = "",
                                IsDeleted = false,
                                IsRead = false,
                                To = follower.user,
                                PostLink = postInfo.Post,
                            };

                            follower.user.Alerts.Add(alert);

                            if (follower.user.Settings == null)
                            {
                                follower.user.Settings = new UserSettings();
                            }
                        }
                    }

                    db.SaveChanges();
                }
                return true;
            }
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public bool AlertNewChat(int chatId)
        {
            return true;
        }
    }
}