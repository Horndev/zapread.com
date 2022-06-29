using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUnresolvedModReportsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<ReportViewModel> Reports;

        /// <summary>
        /// 
        /// </summary>
        public class ReportViewModel
        {
            /// <summary>
            /// 
            /// </summary>
            public Guid ReportId { get; set; }
            
            /// <summary>
            /// 
            /// </summary>
            public int PostId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public long CommentId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int ReportType { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GroupName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ReportedByName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool IsStarted { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public DateTime TimeStamp { get; set; }
        }
    }
}