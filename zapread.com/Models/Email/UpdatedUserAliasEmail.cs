using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdatedUserAliasEmail : Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public string OldUserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NewUserName { get; set; }
    }
}