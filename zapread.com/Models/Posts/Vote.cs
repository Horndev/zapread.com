using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Posts
{
    /// <summary>
    /// This is the REST call model
    /// </summary>
    public class Vote
    {
        /// <summary>
        /// The post or comment Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// direction of vote. 0 = down 1 = up
        /// </summary>
        public int d { get; set; }

        /// <summary>
        /// the amount of the vote in Satoshi
        /// </summary>
        public int a { get; set; }

        /// <summary>
        /// The transaction id if anonymous vote
        /// </summary>
        public int tx { get; set; }
    }
}