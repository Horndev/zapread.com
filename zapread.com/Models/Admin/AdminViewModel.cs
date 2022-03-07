using zapread.com.Models.Database;

namespace zapread.com.Models.Admin
{
    /// <summary>
    /// 
    /// </summary>
    public class AdminViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public ZapReadGlobals Globals { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double PendingGroupToDistribute { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNTotalDeposited { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNTotalWithdrawn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNFeesPaid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNLocalBalance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNRemoteBalance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double LNCapacity { get; set; }
    }
}