﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace zapread.com.App_Start
{
    /// <summary>
    /// Configure webapi2
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // https://www.wiliam.com.au/wiliam-blog/enabling-session-state-in-web-api
            // This enables us to access the MVC5 context from WebAPI
            var httpControllerRouteHandler = typeof(HttpControllerRouteHandler).GetField("_instance",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (httpControllerRouteHandler != null)
            {
                httpControllerRouteHandler.SetValue(null,
                    new Lazy<HttpControllerRouteHandler>(() => new SessionHttpControllerRouteHandler(), true));
            }

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        /// <summary>
        /// Read only for the session state
        /// </summary>
        public class SessionControllerHandler : HttpControllerHandler, IReadOnlySessionState
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="routeData"></param>
            public SessionControllerHandler(RouteData routeData)
                : base(routeData)
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SessionHttpControllerRouteHandler : HttpControllerRouteHandler
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="requestContext"></param>
            /// <returns></returns>
            protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new SessionControllerHandler(requestContext.RouteData);
            }
        }
    }
}