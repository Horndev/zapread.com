using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Account.Transactions
{
    public class LightningTransactionsPageResponse : ZapReadResponse
    {
        public IEnumerable<LightningTransactionsInfo> data { get; set; }
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
    }

    public class LightningTransactionsInfo
    {
        public int Id { get; set; }
        public DateTime? Time { get; set; }
        public bool Type { get; set; }
        public long Amount { get; set; }
        public string Memo { get; set; }
        public bool IsSettled { get; set; }
        public bool IsLimbo { get; set; }
    }


}