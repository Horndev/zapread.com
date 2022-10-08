using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TestEmail : Postal.Email
    {
        /// <summary>
        /// 
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Comment { get; set; }
    }
}