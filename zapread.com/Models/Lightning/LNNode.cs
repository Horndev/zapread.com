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
        [Key]
        public int Id { get; set; }
        public string Alias { get; set; }
        public string Version { get; set; }
        public bool IsTestnet { get; set; }
        public string PubKey { get; set; }
        public string Address { get; set; }

        [InverseProperty("Node")]
        public virtual ICollection<LNNodeVersionHistory> VersionHistory { get; set; }
        public virtual ICollection<LNChannel> Channels { get; set; }
    }

    /// <summary>
    /// This class records the version history of a LN Node
    /// </summary>
    public class LNNodeVersionHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string Version { get; set; }
        [InverseProperty("VersionHistory")]
        public virtual LNNode Node { get; set; }
    }

    public class LNChannel
    {
        [Key]
        public int Id { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLocalInitiator { get; set; }
        public string RemotePubKey { get; set; }
        public string RemoteAlias { get; set; }
        public string ChannelPoint { get; set; }
        public string ChannelId { get; set; }
        public Int64 TotalSent_MilliSatoshi { get; set; }
        public Int64 TotalReceived_MilliSatoshi { get; set; }
        public Int64 Capacity_MilliSatoshi { get; set; }
        public Int64 LocalReserve_MilliSatoshi { get; set; }
        public Int64 RemoteReserve_MilliSatoshi { get; set; }
        [InverseProperty("Channel")]
        public virtual ICollection<LNChannelHistory> ChannelHistory { get; set; }
    }

    public class LNChannelHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool IsOnline { get; set; }
        public Int64 LocalBalance_MilliSatoshi { get; set; }
        public Int64 RemoteBalance_MilliSatoshi { get; set; }
        [InverseProperty("ChannelHistory")]
        public LNChannel Channel { get; set; }
    }
}