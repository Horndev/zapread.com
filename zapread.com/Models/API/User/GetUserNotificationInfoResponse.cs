using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUserNotificationInfoResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public Database.UserSettings Settings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> Languages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> KnownLanguages { get; set; }
    }
}