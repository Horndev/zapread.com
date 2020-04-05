using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API
{
    /// <summary>
    /// Response to a user Balance request
    /// </summary>
    public class UserBalanceResponse : ZapReadResponse
    {
        /// <summary>
        /// User balance in Satoshis
        /// </summary>
        public string balance { get; set; }
    }
}