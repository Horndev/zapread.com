using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Owin.Security.Providers.LnAuth
{
    public class LnAuthAuthenticationOptions : AuthenticationOptions
    {
        public class LnAuthAuthenticationEndpoints
        {
            /// <summary>
            /// Endpoint which is used to redirect users to request xx access
            /// </summary>
            /// <remarks>
            /// Defaults to https://domain.com/lnauth/auth
            /// </remarks>
            public string AuthorizationEndpoint { get; set; }

            /// <summary>
            /// Endpoint which is used to exchange code for access token
            /// </summary>
            /// <remarks>
            /// Defaults to https://xxx/access_token
            /// </remarks>
            public string TokenEndpoint { get; set; }

            /// <summary>
            /// Endpoint which is used to obtain user information after authentication
            /// </summary>
            /// <remarks>
            /// Defaults to https://api.zapread.com/user
            /// </remarks>
            public string UserInfoEndpoint { get; set; }
        }

        private const string AuthorizationEndPoint = "https://zapread.com/lnauth/auth"; //"http://localhost:27543/lnauth/auth"; //"http://192.168.0.172:27543/lnauth/auth";//"https://zapread.com/lnauth/auth";
        private const string TokenEndpoint = "https://zapread.com/lnauth/login/oauth/access_token"; // Not used
        private const string UserInfoEndpoint = "https://api.zapread.com/user"; // Not used

        /// <summary>
        ///     Gets or sets the a pinned certificate validator to use to validate the endpoints used
        ///     in back channel communications belong to xx.
        /// </summary>
        /// <value>
        ///     The pinned certificate validator.
        /// </value>
        /// <remarks>
        ///     If this property is null then the default certificate checks are performed,
        ///     validating the subject name and if the signing chain is a trusted party.
        /// </remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        ///     The HttpMessageHandler used to communicate with xx.
        ///     This cannot be set at the same time as BackchannelCertificateValidator unless the value
        ///     can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        ///     Gets or sets timeout value in milliseconds for back channel communications with xx.
        /// </summary>
        /// <value>
        ///     The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        ///     The request path within the application's base path where the user-agent will be returned.
        ///     The middleware will process this request when it arrives.
        ///     Default value is "/lnauth/signin".
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        ///     Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public string DomainURL { get; set; }

        /// <summary>
        ///     Gets or sets the xx supplied Client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets the xx supplied Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the sets of OAuth endpoints used to authenticate against xx.  Overriding these endpoints allows you to use xx Enterprise for
        /// authentication.
        /// </summary>
        public LnAuthAuthenticationEndpoints Endpoints { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="ILnAuthAuthenticationProvider" /> used in the authentication events
        /// </summary>
        public ILnAuthAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// A list of permissions to request.
        /// </summary>
        public IList<string> Scope { get; private set; }

        /// <summary>
        ///     Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user
        ///     <see cref="System.Security.Claims.ClaimsIdentity" />.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        ///     Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        ///     Initializes a new <see cref="LnAuthAuthenticationOptions" />
        /// </summary>
        public LnAuthAuthenticationOptions()
            : base("LnAuth")
        {
            Caption = Constants.DefaultAuthenticationType;
            CallbackPath = new PathString("/lnauth/callback");
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string>
            {
                "user"
            };
            BackchannelTimeout = TimeSpan.FromSeconds(120);
            Endpoints = new LnAuthAuthenticationEndpoints
            {
                AuthorizationEndpoint = AuthorizationEndPoint,
                TokenEndpoint = TokenEndpoint,
                UserInfoEndpoint = UserInfoEndpoint
            };
        }
    }
}