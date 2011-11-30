using AutoMapper;
using Raven.Client;
using SpeedyMailer.ControlRoom.Website.ViewModels.Builders;
using SpeedyMailer.ControlRoom.Website.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.NinjectProvider;

[assembly: WebActivator.PreApplicationStartMethod(typeof(SpeedyMailer.ControlRoom.Website.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(SpeedyMailer.ControlRoom.Website.App_Start.NinjectMVC3), "Stop")]

namespace SpeedyMailer.ControlRoom.Website.App_Start
{
    using System.Reflection;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Mvc;

    public static class NinjectMVC3 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
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
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel
                .Bind<IEmailCSVParser>().To<EmailCSVParser>();

            kernel
                .Bind<IViewModelBuilderWithBuildParameters<EmailUploadViewModel, IEmailCSVParser>>()
                .To<EmailUploadViewModelBuilder>();

            kernel
                .Bind<IEmailsRepository>()
                .To<EmailsRepository>();
            kernel.Bind<IDocumentStore>().ToProvider<RavenDocumentStoreProvider>();
            kernel.Bind<IMappingEngine>().ToConstant(Mapper.Engine);
        }        
    }
}
