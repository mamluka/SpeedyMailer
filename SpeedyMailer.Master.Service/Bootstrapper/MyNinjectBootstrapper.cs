using Nancy.Bootstrappers.Ninject;
using Ninject;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Core.DataAccess.Drone;
using SpeedyMailer.Core.DataAccess.Fragments;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Utilities.Domain.Email;
using SpeedyMailer.Master.Service.Emails;
using SpeedyMailer.Master.Service.MailDrones;

namespace SpeedyMailer.Master.Service.Bootstrapper
{
    public class MyNinjectBootstrapper : NinjectNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            existingContainer.Bind<IMailDroneRepository>().To<MailDroneRepository>();
            existingContainer.Bind<IMailDroneService>().To<MailDroneService>();
            existingContainer.Bind<IPoolMailOporations>().To<PoolMailOporations>();
            existingContainer.Bind<IEmailPoolService>().To<EmailPoolService>();
            existingContainer.Bind<IDocumentStore>().ToProvider<RavenDocumentStoreProvider>().InThreadScope();

            existingContainer.Bind<IDroneMailOporations>().To<DroneMailOporations>();

            existingContainer.Bind<IRestClient>().To<RestClient>();
            existingContainer.Bind<IEmailSourceWeaver>().To<EmailSourceWeaver>();
            existingContainer.Bind<IUrlCreator>().To<UrlCreator>();
            existingContainer.Bind<IContactsRepository>().To<ContactsRepository>();
            existingContainer.Bind<IFragmentRepository>().To<FragmentRepository>();
        }
    }
}