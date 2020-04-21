using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace realtime.zapread.com.Models.API
{
    public class PaymentMessage
    {
        public string toUserId { get; set; }
        public string invoice { get; set; }
        public double balance { get; set; }
        public int txid { get; set; }
    }
}
