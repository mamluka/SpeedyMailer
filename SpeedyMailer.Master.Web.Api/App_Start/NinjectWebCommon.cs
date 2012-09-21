using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Web.Core;

[assembly: WebActivator.PreApplicationStartMethod(typeof(SpeedyMailer.Master.Web.Api.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(SpeedyMailer.Master.Web.Api.App_Start.NinjectWebCommon), "Stop")]

namespace SpeedyMailer.Master.Web.Api.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
	        kernel.Bind<SettingsProvider>().ToConstant(new SettingsProvider(kernel));
            
            RegisterServices(kernel);
            return kernel;
        }

	    /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
	        ContainerBootstrapper.Bootstrap(kernel)
		        .Analyze(x => x.AssembiesContaining(new[]
			        {
				        typeof (CoreAssemblyMarker),
				        typeof (WebCoreAssemblyMarker),
						typeof(WebApiMarker),
						typeof(IRestClient),
			        }))
					.BindInterfaceToDefaultImplementation()
					.DefaultConfiguration()
					.NoDatabase()
					.Settings(x=> x.UseJsonFiles())
					.Done();
        }        
    }
}
