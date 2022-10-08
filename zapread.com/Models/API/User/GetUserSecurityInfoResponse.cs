using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUserSecurityInfoResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsEmailAuthenticatorEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool TwoFactor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsGoogleAuthenticatorEnabled { get; set; }
    }
}