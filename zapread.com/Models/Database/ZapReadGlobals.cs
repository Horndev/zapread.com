using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zapread.com.Models.Database
{
    public class ZapReadGlobals
    {
        [Key]
        public int Id { get; set; }

        public double ZapReadEarnedBalance { get; set; }

        public double ZapReadTotalEarned { get; set; }

        public double ZapReadTotalWithdrawn { get; set; }

        public ICollection<LNTransaction> LNWithdraws { get; set; }

        // Funds waiting to be distributed to users
        public double CommunityEarnedToDistribute { get; set; }

        public double TotalEarnedCommunity { get; set; }

        public double TotalDepositedCommunity { get; set; }

        public double TotalWithdrawnCommunity { get; set; }

        // The URL address of the Lightning node
        public string LnMainnetHost { get; set; }

        public string LnPubkey { get; set; }

        // The macaroons (Hex Encoded)
        public string LnMainnetMacaroonInvoice { get; set; }
        public string LnMainnetMacaroonRead { get; set; }
        public string LnMainnetMacaroonAdmin { get; set; }
    }
}