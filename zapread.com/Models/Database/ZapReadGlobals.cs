using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Website database settings.  There should be only one entry in the table.
    /// </summary>
    public class ZapReadGlobals
    {
        /// <summary>
        /// Database key.  Note, there should be only one (production)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Tracks how much has been earned in the current period.
        /// </summary>
        public double ZapReadEarnedBalance { get; set; }

        /// <summary>
        /// How much the website has earned.
        /// This is to be depricated soon
        /// </summary>
        public double ZapReadTotalEarned { get; set; }

        /// <summary>
        /// How much has been taken off the platform
        /// </summary>
        public double ZapReadTotalWithdrawn { get; set; }

        /// <summary>
        /// Recording of platform Lightning Network transactions
        /// </summary>
        public ICollection<LNTransaction> LNWithdraws { get; set; }

        /// <summary>
        /// Platform earning events
        /// </summary>
        public virtual ICollection<EarningEvent> EarningEvents { get; set; }

        /// <summary>
        /// Platform spending events
        /// e.g. Bounty, reward, payment.
        /// </summary>
        public virtual ICollection<SpendingEvent> SpendingEvents { get; set; }

        /// <summary>
        /// Funds waiting to be distributed to users
        /// </summary>
        public double CommunityEarnedToDistribute { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalEarnedCommunity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalDepositedCommunity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double TotalWithdrawnCommunity { get; set; }

        /// <summary>
        /// The URL address of the Lightning node
        /// </summary>
        public string LnMainnetHost { get; set; }

        /// <summary>
        /// Node pubkey
        /// </summary>
        public string LnPubkey { get; set; }

        /// <summary>
        /// Macaroon hex encoded
        /// </summary>
        public string LnMainnetMacaroonInvoice { get; set; }

        /// <summary>
        /// Macaroon hex encoded
        /// </summary>
        public string LnMainnetMacaroonRead { get; set; }

        /// <summary>
        /// Macaroon hex encoded
        /// </summary>
        public string LnMainnetMacaroonAdmin { get; set; }
    }
}