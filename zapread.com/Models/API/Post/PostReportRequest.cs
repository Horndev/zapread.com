using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Post
{
    /// <summary>
    /// 
    /// </summary>
    public class PostReportRequest
    {
        /// <summary>
        /// Type of report
        /// </summary>
        public int ReportType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }
    }
}