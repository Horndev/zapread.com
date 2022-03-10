using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Lightning
{
    /// <summary>
    /// Describes an Lightning Node
    /// </summary>
    public class LNNode
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsTestnet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PubKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Node")]
        public virtual ICollection<LNNodeVersionHistory> VersionHistory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<LNChannel> Channels { get; set; }
    }

    /// <summary>
    /// This class records the version history of a LN Node
    /// </summary>
    public class LNNodeVersionHistory
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("VersionHistory")]
        public virtual LNNode Node { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LNChannel
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPrivate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLocalInitiator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RemotePubKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RemoteAlias { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ChannelPoint { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 TotalSent_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 TotalReceived_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 Capacity_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 LocalReserve_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 RemoteReserve_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Channel")]
        public virtual ICollection<LNChannelHistory> ChannelHistory { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LNChannelHistory
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 LocalBalance_MilliSatoshi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 RemoteBalance_MilliSatoshi { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("ChannelHistory")]
        public LNChannel Channel { get; set; }
    }
}