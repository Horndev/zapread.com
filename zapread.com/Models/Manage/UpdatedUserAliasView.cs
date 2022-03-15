using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Manage
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatedUserAliasView
    {
        /// <summary>
        /// 
        /// </summary>
        public string NewUserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OldUserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Database.User User { get; set; }
    }
}