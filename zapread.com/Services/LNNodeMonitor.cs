using LightningLib.lndrpc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using zapread.com.Database;

namespace zapread.com.Services
{
    public class LNNodeMonitor
    {
        public void UpdateHourly()
        {
            using (var db = new ZapContext())
            {
                LndRpcClient lndClient = GetLndClient(db);

                var ni = lndClient.GetInfo();

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
                    });
                    db.SaveChanges();

                    node = db.LNNodes
                        .Include(n => n.Channels)
                        .Where(n => n.PubKey == ni.identity_pubkey)
                        .FirstOrDefault();
                }

                // Check channels
                var channels = lndClient.GetChannels();

                if (channels != null)
                {
                    foreach (var channel in channels.channels)
                    {
                        // Check if channel in db
                        if (node.Channels.Where(cn => cn.ChannelId == channel.chan_id).Any())
                        {
                            // Update channel
                        }
                        else
                        {
                            // New channel
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

        public void UpdateDaily()
        {

        }

        public void UpdateWeekly()
        {

        }

        public void UpdateMonthly()
        {

        }
    }
}