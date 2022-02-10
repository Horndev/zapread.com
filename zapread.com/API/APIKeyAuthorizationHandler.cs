using Microsoft.Owin.Security.ApiKey.Contexts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;

namespace zapread.com.API
{
    //https://dzone.com/articles/api-key-user-aspnet-web-api

    // This could be an alternative.
    /// <summary>
    /// Handler for validating API Keys
    /// 
    /// [TODO] How to use
    /// </summary>
    public static class APIKeyAuthorizationHandler
    {
        /// <summary>
        /// Check that the key in the identity context is valid
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task ValidateIdentity(ApiKeyValidateIdentityContext context)
        {
            using (var db = new ZapContext())
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var submittedKey = context.ApiKey;

                var isValidKey = await db.APIKeys
                    .Where(k => k.Key == submittedKey)
                    .AnyAsync().ConfigureAwait(true);

                if (isValidKey)
                {
                    context.Validate();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext context)
        {
            using (var db = new ZapContext())
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var submittedKey = context.ApiKey;

                var keyRoles = await db.APIKeys
                    .Where(k => k.Key == submittedKey)
                    .Select(k => new
                    {
                        k.Roles,
                        k.User.Name,
                        k.User.AppId,
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(true);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, keyRoles.Name),
                    new Claim(ClaimTypes.NameIdentifier, keyRoles.AppId)
                };

                foreach(var r in keyRoles.Roles.Split(','))
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }

                return claims;
            }
        }
    }
}