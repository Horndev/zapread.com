using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUserFollowInfoResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UserFollowView> TopFollowing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<UserFollowView> TopFollowers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}