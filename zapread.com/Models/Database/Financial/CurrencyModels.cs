using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Finance
{
    /// <summary>
    /// 
    /// </summary>
    public class ExchangeRate
    {
        /// <summary>
        /// 
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        DateTime TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        double BTCperX { get; set; }
    }
}