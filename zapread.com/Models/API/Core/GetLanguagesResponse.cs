using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Core
{
    /// <summary>
    /// Response for the api core languages list command
    /// </summary>
    public class GetLanguagesResponse: ZapReadResponse
    {
        /// <summary>
        /// // List of languages
        /// </summary>
        public List<string> Languages { get; set; }
    }
}