using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// 
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Tags")]
        public virtual ICollection<Post> Posts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Tags")]
        public virtual ICollection<Comment> Comments { get; set; }
    }
}