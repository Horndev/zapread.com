namespace zapread.com.Models.Admin
{
    /// <summary>
    /// 
    /// </summary>
    public class AdminLightningViewModel
    {
        /// <summary>
        /// // The URL address of the Lightning node
        /// </summary>
        public string LnMainnetHost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LnPubkey { get; set; }

        /// <summary>
        /// // The macaroons (Hex Encoded)
        /// </summary>
        public string LnMainnetMacaroonInvoice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LnMainnetMacaroonRead { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LnMainnetMacaroonAdmin { get; set; }
    }
}