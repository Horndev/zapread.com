using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace zapread.com.Services
{
    /// <summary>
    /// Service handler for Zapread
    /// 
    /// Events
    /// 
    /// New Post
    ///     [ ] Email Followers
    ///     [ ] Alert Followers
    /// Post Comment
    /// Post Comment Reply
    /// 
    /// </summary>
    public class EventService : IEventService
    {
        /// <summary>
        /// Handle when a new comment is made on a post (root comment)
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest">If true, then will simulate user interactions such as sending mail.</param>
        /// <returns></returns>
        public async Task<bool> OnPostCommentAsync(long commentId, bool isTest = false)
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailPostComment(commentId, isTest));
            BackgroundJob.Enqueue<AlertsService>(methodCall: x => x.AlertPostComment(commentId, isTest));
            await NotificationService.NotifyPostCommentToAuthor(commentId);
            await NotificationService.NotifyPostCommentToFollowers(commentId);

            return true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public async Task<bool> OnCommentReplyAsync (long commentId, bool isTest = false)
        {
            // Send emails out as needed
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailPostCommentReply(commentId, isTest));
            BackgroundJob.Enqueue<AlertsService>(methodCall: x => x.AlertPostCommentReply(commentId));
            await NotificationService.NotifyPostCommentReplyToAuthor(commentId);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdFollowed"></param>
        /// <param name="userIdFollowing"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public async Task<bool> OnNewUserFollowingAsync(int userIdFollowed, int userIdFollowing, bool isTest = false)
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailUserNewFollower(userIdFollowed, userIdFollowing, isTest));

            await NotificationService.NotifyNewUserFollowing(userIdFollowed, userIdFollowing);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="userAppId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public async Task<bool> OnUserMentionedInComment(long commentId, bool isTest = false)
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailUserMentionedInComment(commentId, isTest));

            await NotificationService.NotifyUserMentionedInComment(commentId);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public async Task<bool> OnNewPostAsync(int postId, bool isTest = false)
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailNewPostToFollowers(postId, isTest));
            BackgroundJob.Enqueue<AlertsService>(methodCall: x => x.AlertNewPost(postId));
            await NotificationService.NotifyNewPostToFollowers(postId);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public async Task<bool> OnNewChatAsync(int chatId, bool isTest = false)
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailNewChatToUser(chatId, isTest));

            // Sends a popup
            await NotificationService.NotifyNewChat(chatId);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> OnUpdateUserAliasAsync(int userId, string oldName, string newName, bool isTest = false)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailUpdatedUserAlias(userId, oldName, newName, isTest));
            
            return true;
        }
    }
}