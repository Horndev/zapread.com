using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserBalanceInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double SpendOnlyBalance { get; set; }

        /// <summary>
        /// If user can click to vote
        /// </summary>
        public bool QuickVoteOn { get; set; }

        /// <summary>
        /// How much to vote each click
        /// </summary>
        public int QuickVoteAmount { get; set; }
    }
}