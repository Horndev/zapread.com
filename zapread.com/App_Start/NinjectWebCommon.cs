[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(zapread.com.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(zapread.com.App_Start.NinjectWebCommon), "Stop")]

namespace zapread.com.App_Start
{
    using System;
    using System.Web;
    using global::DI.Ninject.Modules;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;
    using zapread.com.Services;

    /// <summary>
    /// Ninject dependency injection
    /// </summary>
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application.
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load(new MvcSiteMapProviderModule());
            kernel.Bind<LightningPayments>().ToSelf().InSingletonScope();
            kernel.Bind<ILightningPayments>().ToMethod(ctx => ctx.Kernel.Get<LightningPayments>());

            kernel.Bind<IEventService>().To<EventService>();
            kernel.Bind<IPointOfSaleService>().To<PointOfSaleService>();
        }
    }
}