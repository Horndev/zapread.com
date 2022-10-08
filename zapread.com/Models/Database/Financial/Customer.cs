using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 0 = Square
        /// </summary>
        public int Provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public User User { get; set; }
    }
}