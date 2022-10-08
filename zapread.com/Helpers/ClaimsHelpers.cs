using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using zapread.com.Database;
using zapread.com.Models.Database;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class ClaimsHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name = "userAppId"></param>
        /// <param name = "User"></param>
        /// <returns></returns>
        public static async Task ValidateClaims(string userAppId, IPrincipal User)
        {
            try
            {
                if (userAppId != null)
                {
                    using (var db = new ZapContext())
                    {
                        var us = await db.Users.Where(u => u.AppId == userAppId).Select(u => new
                        {
                        u.Settings.ColorTheme, u.ProfileImage.Version, u.AppId, }).FirstOrDefaultAsync().ConfigureAwait(true);
                        if (us != null)
                        {
                            User.AddUpdateClaim("ColorTheme", us.ColorTheme ?? "light");
                            User.AddUpdateClaim("ProfileImageVersion", us.Version.ToString(CultureInfo.InvariantCulture));
                            User.AddUpdateClaim("UserAppId", us.AppId);
                        }
                    }
                }
            }
            catch (Exception)
            {
            //TODO: handle (or fix test for HttpContext.Current.GetOwinContext().Authentication mocking)
            }
        }
    }
}