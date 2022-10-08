using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// mime
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public byte[] Content { get; set; }
    }
}