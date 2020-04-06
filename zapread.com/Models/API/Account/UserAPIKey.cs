using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// An API key used to 
    /// </summary>
    public class UserAPIKey
    {
        /// <summary>
        /// Key in the form of a GUID
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Comma-separated list of roles assigned to key
        /// </summary>
        public string Roles { get; set; }
    }
}