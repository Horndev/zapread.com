using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPointOfSaleService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="token"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="customerEmail"></param>
        /// <param name="firstName"></param>
        /// <param name="LastName"></param>
        /// <returns></returns>
        Task<bool> Subscribe(string userAppId, string token, string verificationToken, string planId, string customerEmail, string firstName, string LastName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAppId"></param>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        Task<bool> Unsubscribe(string userAppId, string subscriptionId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> SyncSubscriptions();
    }
}