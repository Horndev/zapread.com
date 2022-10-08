using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Account
{
    /// <summary>
    /// View model for logging in with Lnauth
    /// </summary>
    public class LNAuthLoginView
    {
        /// <summary>
        /// B64 encoded QR code for login
        /// </summary>
        public string QrImageBase64 { get; set; }

        /// <summary>
        /// one-time secret for login
        /// </summary>
        public string k1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string client_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string redirect_uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string dataStr { get; set; }
    }
}