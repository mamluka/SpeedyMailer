using Nancy.Bootstrappers.Ninject;
using Ninject;
using Ninject.Extensions.Conventions;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.Core.NinjectProvider;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.Core.Emails;
using SpeedyMailer.EmailPool.Master.MailDrones;

namespace SpeedyMailer.EmailPool.Master.Bootstrapper
{
    public class MyNinjectBootstrapper:NinjectNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            
        
            existingContainer.Bind<IMailDroneRepository>().To<MailDroneRepository>();
            existingContainer.Bind<IMailDroneService>().To<MailDroneService>();
            existingContainer.Bind<IPoolMailOporations>().To<PoolMailOporations>();
            existingContainer.Bind<IEmailPoolService>().To<EmailPoolService>();
            existingContainer.Bind<IDocumentStore>().ToProvider<RavenDocumentStoreProvider>();

            existingContainer.Bind<IDroneMailOporations>().To<DroneMailOporations>();

            existingContainer.Bind<IRestClient>().To<RestClient>();
            existingContainer.Bind<IEmailSourceWeaver>().To<EmailSourceWeaver>();
            existingContainer.Bind<IUrlCreator>().To<UrlCreator>();
            existingContainer.Bind<IContactsRepository>().To<ContactsRepository>();
            existingContainer.Bind<IFragmentRepository>().To<FragmentRepository>();



        }
    }
}