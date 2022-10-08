using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateQuickVoteRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public bool QuickVoteOn { get; set; }

        /// <summary>
        /// Amount in Satoshi
        /// </summary>
        public int QuickVoteAmount { get; set; }
    }
}