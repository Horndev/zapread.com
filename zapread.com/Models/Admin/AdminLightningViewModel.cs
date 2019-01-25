using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Admin
{
    public class AdminLightningViewModel
    {
        // The URL address of the Lightning node
        public string LnMainnetHost { get; set; }

        public string LnPubkey { get; set; }

        // The macaroons (Hex Encoded)
        public string LnMainnetMacaroonInvoice { get; set; }
        public string LnMainnetMacaroonRead { get; set; }
        public string LnMainnetMacaroonAdmin { get; set; }
    }
}