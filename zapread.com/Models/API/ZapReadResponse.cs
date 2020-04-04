using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API
{
    /// <summary>
    /// API response
    /// </summary>
    public class ZapReadResponse
    {
        /// <summary>
        /// True if the request completed successfully.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Message from the sever (usually when not successful)
        /// </summary>
        public string message { get; set; }
    }
}