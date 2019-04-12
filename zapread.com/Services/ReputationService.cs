using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Services
{
    public static class ReputationService
    {
        public static double GetReputationAdjustedAmount(Int64 amount, Int64 targetRep, Int64 actorRep)
        {
            var vs = 1;
            var n = actorRep - targetRep;
            if (amount < 0)
            {
            //    vs = -1;
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
                ds = vs;
            return ds;
        }
    }
}