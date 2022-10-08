using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Subscription
{
    /// <summary>
    /// 
    /// </summary>
    public struct SubscriptionTypes
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string FollowedUserNewPost = "NewPost";
        public const string OwnPostComment = "PostComment";
        public const string FollowedPostComment = "FollowedPostComment";
        public const string OwnCommentReply = "CommentReply";
        public const string UserMentionedInComment = "CommentMentioned";
        public const string NewChat = "NewChat";
        public const string NewUserFollowing = "UserFollowing";
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}