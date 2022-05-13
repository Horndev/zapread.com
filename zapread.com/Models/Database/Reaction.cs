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
    /// https://emojipedia.org/star-struck/
    /// 
    /// [X] happy-face
    /// [X] thumbs-up
    /// [X] heart
    /// [X] star-eyes       🤩   [X] Follow someone
    /// [X] raising-hands   🙌   [X] Someone following you
    /// [X] bolt            ⚡   [X] LN Deposit
    /// [X] rocket          🚀   [X] LN Withdraw
    /// [X] green-check     ✅   [X] First Vote
    /// 
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int ReactionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReactionName { get; set; }

        /// <summary>
        /// UTF-8 based emoji.  If null, use Image
        /// </summary>
        public string ReactionIcon { get; set; }

        /// <summary>
        /// Alternative - reaction as an image (custom)
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("AvailableReactions")]
        public virtual ICollection<User> UnlockedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UnlockedAll { get; set; }
    }
}