using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// Response to keys request
    /// </summary>
    public class APIKeysResponse: ZapReadResponse
    {
        /// <summary>
        /// A list of 0+ API keys
        /// </summary>
        public List<UserAPIKey> Keys { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserAppId { get; set; }
    }
}