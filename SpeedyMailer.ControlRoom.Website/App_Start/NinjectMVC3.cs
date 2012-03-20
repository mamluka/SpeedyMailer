using AutoMapper;
using Raven.Client;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Lists;
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
                .Bind<IContactsCSVParser>().To<ContactsCSVParser>();

            kernel
                .Bind<IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser>>()
                .To<UploadListResultsViewModelBuilder>();

            kernel
                .Bind<IViewModelBuilder<UploadListViewModel>>()
                .To<UploadListViewModelBuilder>();

            kernel
               .Bind<IViewModelBuilder<ComposeViewModel>>()
               .To<ComposeViewModelBuilder>();

            kernel
                .Bind<IListRepository>()
                .To<ListRepository>();

            kernel
                .Bind<IContactsRepository>()
                .To<ContactsRepository>();

            kernel.Bind<IEmailSourceParser>().To<EmailSourceParser>();

            kernel.Bind<IEmailRepository>().To<EmailRepository>();

            kernel.Bind<IDocumentStore>().ToProvider<RavenDocumentStoreProvider>();
            kernel.Bind<IMappingEngine>().ToConstant(Mapper.Engine);
            kernel.Bind<IEmailPoolService>().To<EmailPoolService>();
            kernel.Bind<IUrlCreator>().To<UrlCreator>();
            kernel.Bind<IConfigurationManager>().To<ControlRoomConfigurationManager>().InSingletonScope();
        }        
    }
}
