﻿using System;
using System.Collections.Generic;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PostViewModel
    {
        /// <summary>
        /// Only used for debug
        /// </summary>
        public double Hot;

        [Obsolete("Use view model attributes instead of the Post object.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Post Post { get; set; }
        public string PostTitle { get; set; }
        public string Content { get; set; }
        public int PostId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string UserAppId { get; set; }
        public int UserProfileImageVersion { get; set; }
        public int Score { get; set; }
        public DateTime? TimeStamp { get; set; }
        public DateTime? TimeStampEdited { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSticky { get; set; }
        public IEnumerable<PostCommentsViewModel> CommentVms { get; set; }
        public bool ViewerIsMod { get; set; }        // User has moderation privilage on this post
        public bool ViewerUpvoted { get; set; }      // If the user has upvoted this post
        public bool ViewerDownvoted { get; set; }    // If the user has downvoted this post
        public bool ViewerIgnoredUser { get; set; }  // If the user has ignored the user
        public int NumComments { get; set; }
        public bool IsDetailView { get; set; }       // If the post is being viewed by itself
        public bool IsFirstPost { get; set; }        // If the post is the first post on a page

        // Not ideal!
        [Obsolete("Check ViewerIgnoredUser for post status instead.")]
        public List<int> ViewerIgnoredUsers { get; set; }  // If the user has ignored the user
    }

    public class NewPostViewModel
    {
        public Post Post { get; set; }
        public List<string> Languages { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PostsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<PostViewModel> Posts;

        /// <summary>
        /// 
        /// </summary>
        public double UserBalance;
        
        /// <summary>
        /// 
        /// </summary>
        public List<GroupInfo> SubscribedGroups;
        
        /// <summary>
        /// 
        /// </summary>
        public string Sort;
    }

    public class NewPostMsg
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; }
        public bool IsDraft { get; set; }
        public string Language { get; set; }
    }

    public class PostCommentsViewModel
    {
        [Obsolete]
        public Comment Comment { get; set; }
        [Obsolete]
        public Comment ParentComment { get; set; }

        [Obsolete]
        public PostCommentsViewModel ParentCommentVm { get; set; }

        public long ParentCommentId { get; set; }

        public int ParentUserId { get; set; }
        public string ParentUserAppId { get; set; }

        public string ParentUserName { get; set; }
        public int ParentUserProfileImageVersion { get; set; }

        // Used for mailer
        public string ParentCommentText { get; set; }

        [Obsolete]
        public List<Comment> Comments { get; set; } // All comments

        [Obsolete("Use ViewerIgnoredUser instead of this list.")]
        public List<int> ViewerIgnoredUsers;        // If the user has ignored the user
        public bool StartVisible { get; set; }
        public int NestLevel { get; set; }          // How far down the comment nesting is

        public string Text { get; set; }

        public int Score { get; set; }
        public int ParentScore { get; set; }

        public bool IsReply { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? TimeStamp { get; set; }

        public DateTime? TimeStampEdited { get; set; }

        public Int64 CommentId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }
        public string PostTitle { get; set; }
        public int PostId { get; set; }
        public string UserAppId { get; set; }
        public int ProfileImageVersion { get; set; }
        public List<PostCommentsViewModel> CommentVms { get; set; } // All comments

        public bool ViewerUpvoted;      // If the user has upvoted this comment
        public bool ViewerDownvoted;    // If the user has downvoted this comment
        public bool ViewerIgnoredUser;  // If the user has ignored the user
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}