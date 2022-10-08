using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// 
    /// </summary>
    public class GetBannerAlertsResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<BannerAlertItem> Alerts { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BannerAlertItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsGlobalSend { get; set; }
    }
}