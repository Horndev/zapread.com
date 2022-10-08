using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Tag
{
    /// <summary>
    /// 
    /// </summary>
    public class GetTagInfoResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public TagItem Tag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLoggedIn { get; set; }
    }
}