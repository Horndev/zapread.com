using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.User
{
    /// <summary>
    /// API Return for /api/v1/user/referralstats
    /// </summary>
    public class GetRefStatsResponse
    {
        /// <summary>
        /// The number of users that were referred
        /// </summary>
        public int TotalReferred { get; set; }

        /// <summary>
        /// The number of users that were referred and actively paying bonuses
        /// </summary>
        public int TotalReferredActive { get; set; }

        /// <summary>
        /// User ID of user which signed up this user
        /// </summary>
        public string ReferredByAppId { get; set; }

        /// <summary>
        /// Is this user actively paying out Referral bonuses
        /// </summary>
        public bool IsActive { get; set; }
    }
}