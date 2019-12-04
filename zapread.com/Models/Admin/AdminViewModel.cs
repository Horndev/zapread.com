using zapread.com.Models.Database;

namespace zapread.com.Models.Admin
{
    public class AdminViewModel
    {
        public ZapReadGlobals Globals { get; set; }

        public double PendingGroupToDistribute { get; set; }
        public double LNTotalDeposited { get; set; }
        public double LNTotalWithdrawn { get; set; }
        public double LNFeesPaid { get; set; }

        public double LNLocalBalance { get; set; }

        public double LNRemoteBalance { get; set; }
        public double LNCapacity { get; set; }
    }
}