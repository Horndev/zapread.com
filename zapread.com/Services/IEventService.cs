using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnPostCommentAsync(long commentId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnCommentReplyAsync(long commentId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnNewPostAsync(int postId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnNewChatAsync(int chatId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnUpdateUserAliasAsync(int userId, string oldName, string newName, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="userAppId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnUserMentionedInComment(long commentId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdFollowed"></param>
        /// <param name="userIdFollowing"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnNewUserFollowingAsync(int userIdFollowed, int userIdFollowing, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnNewGroupModGrantedAsync(int groupId, int userId, bool isTest = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> OnNewGroupAdminGrantedAsync(int groupId, int userId, bool isTest = false);
    }
}