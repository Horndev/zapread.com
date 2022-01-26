using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Finance
{
    public class ExchangeRate
    {
        int Id { get; set; }

        DateTime TimeStamp { get; set; }

        string Currency { get; set; }

        double BTCperX { get; set; }
    }
}