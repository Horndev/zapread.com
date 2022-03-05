using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class APIKey
    {
        /// <summary>
        /// Key in the form of a GUID
        /// </summary>
        [Key]
        public string Key { get; set; }

        /// <summary>
        /// Comma separated list of roles assigned to key
        /// </summary>
        [Required]
        public string Roles { get; set; }

        /// <summary>
        /// The user who owns this key.
        /// </summary>
        [InverseProperty("APIKeys")]
        public virtual User User { get; set; }
    }
}