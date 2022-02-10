using System;

namespace zapread.com.Services
{
    /// <summary>
    /// Service to manage the adjustment to vote scores based on reputation
    /// </summary>
    public static class ReputationService
    {
        /// <summary>
        /// Get the reputation adjustment as a function of amount transacted in Satoshi, and the participant reputations.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="targetRep"></param>
        /// <param name="actorRep"></param>
        /// <returns></returns>
        public static double GetReputationAdjustedAmount(Int64 amount, Int64 targetRep, Int64 actorRep)
        {
            var vs = 1;
            //var voteup = 1;
            var n = actorRep - targetRep;
            if (amount < 0)
            {
                //    voteup = -1;
                //    n = -1 * n;
            }
            var s = 100000; // scale up
            var sign = 1;
            if (n < 0)
            {
                s = 10000; // scale down
                sign = -1;
            }
            var q = Math.Abs(n);
            var z = Convert.ToDouble(q + s) / Convert.ToDouble(s);
            var d = 1.0 - (1 / z);
            var w = 1.0 + sign * Math.E * Math.Pow(d, z);
            var ds = Math.Round(amount * w, MidpointRounding.AwayFromZero);
            if (ds == 0)
                ds = vs*sign;
            return ds;
        }
    }
}