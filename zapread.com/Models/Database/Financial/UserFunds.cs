using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public class UserFunds
    {
        [Key]
        public int Id { get; set; }
        public double TotalEarned { get; set; }
        public double Balance { get; set; }

        /// <summary>
        /// These are funds the user may have as pending withdraw or deposit.  Not spendable.
        /// </summary>
        public double LimboBalance { get; set; }

        public bool IsWithdrawLocked { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}