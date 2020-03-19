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
    public static class ClaimsHelpers
    {
        public static async Task ValidateClaims(int userId, IPrincipal User)
        {
            try
            {
                if (userId > 0)
                {
                    using (var db = new ZapContext())
                    {
                        var us = await db.Users
                            .Where(u => u.Id == userId)
                            .Select(u => new
                            {
                                u.Settings.ColorTheme,
                                u.ProfileImage.Version,
                                u.AppId,
                            })
                            .FirstOrDefaultAsync().ConfigureAwait(true);

                        User.AddUpdateClaim("ColorTheme", us.ColorTheme ?? "light");
                        User.AddUpdateClaim("ProfileImageVersion", us.Version.ToString(CultureInfo.InvariantCulture));
                        User.AddUpdateClaim("UserAppId", us.AppId);
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