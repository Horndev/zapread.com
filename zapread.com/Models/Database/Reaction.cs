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
    /// [*] Emoji Name     Emoji [*] AchievementService [*] Zapread Database    Achievement Name
    /// 
    /// [X] happy-face
    /// [X] thumbs-up
    /// [X] heart
    /// [X] star-eyes       🤩   [X][X] Follow someone
    /// [X] raising-hands   🙌   [X][X] Someone following you
    /// [X] bolt            ⚡   [X][X] LN Deposit
    /// [X] rocket          🚀   [X][X] LN Withdraw
    /// [X] green-check     ✅   [X][X] First Vote
    /// 
    /// [X] rainbow         🌈   [X][ ] Spend1000
    /// [X] zzz             💤   [X][ ] Spend10000
    /// [X] rofl            🤣   [X][ ] Spend100000
    /// [X] thumbs-down     👎   [X][ ] Spend500000
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