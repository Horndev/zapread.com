using LightningLib.lndrpc;

namespace zapread.com.Services
{
    /// <summary>
    /// Manages payments over the lightning network
    /// </summary>
    public interface ILightningPayments
    {
        void RecordNodeWithdraw(string node);

        /// <summary>
        /// Returns true if the specified node (pub_key) has been marked as banned.  Nodes are banned when they are used in attacks.
        /// </summary>
        /// <param name="node">node pub_key</param>
        /// <param name="message">out - optional user message related to ban</param>
        /// <returns>true if banned</returns>
        bool IsNodeBanned(string node, out string message);

        /// <summary>
        /// Submit a request to make a Lightning Network withdrawal for a specified user.
        /// </summary>
        /// <param name="request">LN transaction request object</param>
        /// <param name="userId">account user ID for database lookup</param>
        /// <param name="ip">IP address of requestor (as can be determined)</param>
        /// <param name="lndClient">configured RPC client object</param>
        /// <returns>dynamic object (json serializable) indicating result</returns>
        object TryWithdrawal(Models.LNTransaction request, string userId, string ip, LndRpcClient lndClient);
    }
}
