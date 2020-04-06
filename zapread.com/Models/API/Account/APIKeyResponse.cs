using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// Return a new API key
    /// </summary>
    public class APIKeyResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonRequired]
        public UserAPIKey Key { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserAppId { get; set; }
    }
}