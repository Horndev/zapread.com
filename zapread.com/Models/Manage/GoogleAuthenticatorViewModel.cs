using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Manage
{
    /// <summary>
    /// 
    /// </summary>
    public class GoogleAuthenticatorViewModel
    {
        /// <summary>
        /// Authenticator code
        /// </summary>
        public string Code { get; set; }

        public string SecretKey { get; set; }
    }
}