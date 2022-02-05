using System;

namespace Owin.Security.Providers.LnAuth
{
    public static class LnAuthAuthenticationExtensions
    {
        public static IAppBuilder UseLnAuthAuthentication(this IAppBuilder app,
            LnAuthAuthenticationOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            app.Use(typeof(LnAuthAuthenticationMiddleware), app, options);

            return app;
        }

        public static IAppBuilder UseLnAuthAuthentication(this IAppBuilder app, string clientId, string clientSecret)
        {
            return app.UseLnAuthAuthentication(new LnAuthAuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            });
        }
    }
}