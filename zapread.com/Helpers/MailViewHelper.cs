using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Helpers
{
    /// <summary>
    /// Helper class to generate strings for the mailer views
    /// </summary>
    public static class MailViewHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string PostURI(int postId, string postTitle, long? commentId = null, int? vote = null)
        {
            //@("https://www.zapread.com/p/" + zapread.com.Services.CryptoService.IntIdToString(Model.PostId) + "/" + Model.PostTitle != null ? Model.PostTitle.MakeURLFriendly() : "" + "/")#cid_@Model.CommentId
            //@("https://www.zapread.com/p/" + zapread.com.Services.CryptoService.IntIdToString(Model.PostId)
            //  + "/" + Model.PostTitle != null ? Model.PostTitle.MakeURLFriendly() : "" + "/")#cid_@Model.CommentId
            return "https://www.zapread.com/p/" 
                + zapread.com.Services.CryptoService.IntIdToString(postId) 
                + "/"
                + (string.IsNullOrEmpty(postTitle) ? "" : postTitle.MakeURLFriendly())
                + "/"
                + (vote.HasValue ? "?vote=" + Convert.ToString(vote.Value) : "")
                + (commentId.HasValue ? ("#cid_" + Convert.ToString(commentId.Value)) : "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string GroupURI(int groupId)
        {
            return "https://www.zapread.com/Group/Detail/" + Convert.ToString(groupId) + "/";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string UserLink(string username)
        {
            //@("https://www.zapread.com/user/" + System.Web.HttpUtility.UrlEncode( Model.ParentUserName.Trim()))
            return "https://www.zapread.com/user/"
                + System.Web.HttpUtility.UrlEncode(username.Trim());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="profileImageVersion"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string UserImageURI(string userAppId, int profileImageVersion = 0, int size = 45)
        {
            //@("https://www.zapread.com/Home/UserImage/?size=45&UserId=" + System.Web.HttpUtility.UrlEncode(Model.UserAppId)
            //  + "&v=" + Convert.ToString(Model.ProfileImageVersion))
            return "https://www.zapread.com/Home/UserImage/?size=" + Convert.ToString(size) + "&UserId="
                + System.Web.HttpUtility.UrlEncode(userAppId)
                + "&v=" + Convert.ToString(profileImageVersion);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string ChatMessageURI(string username)
        {
            return "https://www.zapread.com/Messages/Chat/" + System.Web.HttpUtility.UrlEncode(username.Trim()) + "/";
        }
    }
}