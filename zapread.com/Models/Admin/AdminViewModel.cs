using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.Admin
{
    public class AdminViewModel
    {
        public ZapReadGlobals Globals { get; set; }

        public double PendingGroupToDistribute { get; set; }
        public double LNTotalDeposited { get; set; }
        public double LNTotalWithdrawn { get; set; }
    }
}