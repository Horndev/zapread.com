using LightningLib.lndrpc;
using LightningLib.lndrpc.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Lightning;

namespace zapread.com.Services
{
    /// <summary>
    /// Monitoring of the Lightning Node
    /// </summary>
    public class LNNodeMonitor
    {
        /// <summary>
        /// Method to do hourly status updates
        /// </summary>
        public void UpdateHourly()
        {
            using (var db = new ZapContext())
            {
                LndRpcClient lndClient = GetLndClient(db);

                GetInfoResponse ni;

                try
                {
                    ni = lndClient.GetInfo();
                }
                catch (RestException e)
                {
                    // Unable to communicate with LN Node

                    // TODO - properly log error
                    Console.WriteLine(e.Message);
                    return;
                }

                var node = db.LNNodes
                    .Include(n => n.Channels)
                    .Where(n => n.PubKey == ni.identity_pubkey)
                    .FirstOrDefault();

                if (node == null)
                {
                    db.LNNodes.Add(new Models.Lightning.LNNode()
                    {
                        PubKey = ni.identity_pubkey,
                        Alias = ni.alias,
                        Address = ni.uris[0],
                        Version = ni.version,
                        IsTestnet = ni.chains[0].network != "mainnet", 
                    });
                    db.SaveChanges();

                    node = db.LNNodes
                        .Include(n => n.Channels)
                        .Where(n => n.PubKey == ni.identity_pubkey)
                        .FirstOrDefault();
                }
                else
                {
                    if (node.Version != ni.version)
                    {
                        if (node.VersionHistory == null)
                        {
                            node.VersionHistory = new List<LNNodeVersionHistory>();
                        }
                        node.VersionHistory.Add(new LNNodeVersionHistory()
                        {
                            Node = node,
                            TimeStamp = DateTime.UtcNow,
                            Version = ni.version,
                        });
                        node.Version = ni.version;
                        db.SaveChanges();
                    }
                }

                // Check channels
                var channels = lndClient.GetChannels();

                if (channels != null)
                {
                    foreach (var channel in channels.channels)
                    {
                        // Check if channel in db
                        var nodeChannel = node.Channels.Where(cn => cn.ChannelId == channel.chan_id).FirstOrDefault();

                        if (nodeChannel != null)
                        {
                            // Update channel
                            if (true) // should this be done so frequently?
                            {
                                nodeChannel.TotalSent_MilliSatoshi = Convert.ToInt64(channel.total_satoshis_sent);
                                nodeChannel.TotalReceived_MilliSatoshi = Convert.ToInt64(channel.total_satoshis_received);
                                nodeChannel.IsOnline = channel.active.HasValue ? channel.active.Value : false;

                                // Add history point
                                nodeChannel.ChannelHistory.Add(new LNChannelHistory()
                                {
                                    Channel = nodeChannel,
                                    IsOnline = channel.active.HasValue ? channel.active.Value : false,
                                    LocalBalance_MilliSatoshi = Convert.ToInt64(channel.local_balance),
                                    RemoteBalance_MilliSatoshi = Convert.ToInt64(channel.remote_balance),
                                    TimeStamp = DateTime.UtcNow
                                });

                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            // New channel
                            var newChan = new LNChannel()
                            {
                                Capacity_MilliSatoshi = Convert.ToInt64(channel.capacity),
                                ChannelHistory = new List<LNChannelHistory>(),
                                ChannelId = channel.chan_id,
                                ChannelPoint = channel.channel_point,
                                IsLocalInitiator = channel.initiator.HasValue ? channel.initiator.Value : false,
                                IsOnline = channel.active.HasValue ? channel.active.Value : false,
                                IsPrivate = channel.@private,
                                LocalReserve_MilliSatoshi = Convert.ToInt64(channel.local_chan_reserve_sat),
                                RemotePubKey = channel.remote_pubkey,
                                RemoteReserve_MilliSatoshi = Convert.ToInt64(channel.remote_chan_reserve_sat),
                                TotalReceived_MilliSatoshi = Convert.ToInt64(channel.total_satoshis_received),
                                TotalSent_MilliSatoshi = Convert.ToInt64(channel.total_satoshis_sent),
                                RemoteAlias = "",
                            };
                            
                            node.Channels.Add(newChan);

                            db.SaveChanges();

                            newChan.ChannelHistory.Add(new LNChannelHistory()
                            {
                                Channel = newChan,
                                IsOnline = channel.active.HasValue ? channel.active.Value : false,
                                LocalBalance_MilliSatoshi = Convert.ToInt64(channel.local_balance),
                                RemoteBalance_MilliSatoshi = Convert.ToInt64(channel.remote_balance),
                                TimeStamp = DateTime.UtcNow
                            });

                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static LndRpcClient GetLndClient(ZapContext db)
        {
            var website = db.ZapreadGlobals.Where(gl => gl.Id == 1)
                                .AsNoTracking()
                                .FirstOrDefault();

            LndRpcClient lndClient = new LndRpcClient(
                host: website.LnMainnetHost,
                macaroonAdmin: website.LnMainnetMacaroonAdmin,
                macaroonRead: website.LnMainnetMacaroonRead,
                macaroonInvoice: website.LnMainnetMacaroonInvoice);
            return lndClient;
        }

        /// <summary>
        /// Run once per day
        /// </summary>
        public void UpdateDaily()
        {

        }

        /// <summary>
        /// Run once per week
        /// </summary>
        public void UpdateWeekly()
        {

        }

        /// <summary>
        /// Run once per month
        /// </summary>
        public void UpdateMonthly()
        {

        }
    }
}