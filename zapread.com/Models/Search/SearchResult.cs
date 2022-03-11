using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Search
{

    /// <summary>
    /// 
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id;

        /// <summary>
        /// 
        /// </summary>
        public int PostId;

        /// <summary>
        /// 
        /// </summary>
        public string Type;

        /// <summary>
        /// 
        /// </summary>
        public string Content;

        /// <summary>
        /// 
        /// </summary>
        public string Title;

        /// <summary> 
        /// 
        /// </summary>
        public string EncPostId;

        /// <summary>
        /// 
        /// </summary>
        public string UserAppId;

        /// <summary>
        /// 
        /// </summary>
        public int PostScore;

        /// <summary>
        /// 
        /// </summary>
        public int CommentScore;

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp;

        /// <summary>
        /// 
        /// </summary>
        public string AuthorName;

        /// <summary>
        /// 
        /// </summary>
        public string GroupName;
    }
}