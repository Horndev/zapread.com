using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    public class LightningTransactionsPageResponse : ZapReadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<LightningTransactionsInfo> data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int draw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int recordsTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int recordsFiltered { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LightningTransactionsInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSettled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLimbo { get; set; }
    }


}