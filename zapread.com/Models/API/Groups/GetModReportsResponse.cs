using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GetModReportsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public int NumGroupsModerated { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumReports { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<GroupBalanceInfo> BalanceInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class GroupBalanceInfo
        {
            public int GroupId { get; set; }

            public string GroupName { get; set; }

            public double Balance { get; set; }
        }
    }
}