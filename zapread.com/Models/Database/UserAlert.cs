using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAlert
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Alerts")]
        public User To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Post PostLink { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Comment CommentLink { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsRead { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}