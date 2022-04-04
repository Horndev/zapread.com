using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Services
{
    /// <summary>
    /// Service handler for Zapread
    /// </summary>
    public class EventService
    {
        /// <summary>
        /// Handle when a new comment is made on a post (root comment)
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest">If true, then will simulate user interactions such as sending mail.</param>
        /// <returns></returns>
        public bool OnPostComment(long commentId, bool isTest = false)
        {
            // Send emails out as needed
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailPostComment(commentId, isTest));

            // Send out alerts as needed
            BackgroundJob.Enqueue<AlertsService>(methodCall: x => x.AlertPostComment(commentId, isTest));

            return true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool OnCommentReply(long commentId, bool isTest = false)
        {
            // Send emails out as needed
            BackgroundJob.Enqueue<MailingService>(methodCall: x => x.MailPostCommentReply(commentId, isTest));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="userAppId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public bool OnUserMentionedInComment(long commentId, string userAppId, bool isTest = false)
        {
            return true;
        }
    }
}