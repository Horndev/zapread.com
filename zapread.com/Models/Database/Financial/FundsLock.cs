using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public class FundsLock
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int FundsLockId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UserFundId { get; set; }

        /// <summary>
        /// Navigation property
        /// </summary>
        [ForeignKey("UserFundId")]
        public UserFunds UserFund { get; set; }

        /// <summary>
        /// Enumerated options
        /// </summary>
        public int Reason { get; set; }

        /// <summary>
        /// LN withdraw
        /// </summary>
        public bool WithdrawLocked { get; set; }

        /// <summary>
        /// LN deposit (don't give invoice)
        /// </summary>
        public bool DepositLocked { get; set; }

        /// <summary>
        /// Can't tip/transfer
        /// </summary>
        public bool TransferLocked { get; set; }

        /// <summary>
        /// Can't vote
        /// </summary>
        public bool SpendLocked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStampStarted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStampExpired { get; set; }
    }
}