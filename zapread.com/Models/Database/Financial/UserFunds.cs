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
    public class UserFunds
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double TotalEarned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Balance { get; set; }

        /// <summary>
        /// These are funds the user may have as pending withdraw or deposit.  Not spendable.
        /// </summary>
        public double LimboBalance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsWithdrawLocked { get; set; }

        /// <summary>
        /// Describe any locks
        /// </summary>
        [InverseProperty("UserFund")]
        public ICollection<FundsLock> Locks { get; set; }

        /// <summary>
        /// Versioning for optimistic concurrency
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}