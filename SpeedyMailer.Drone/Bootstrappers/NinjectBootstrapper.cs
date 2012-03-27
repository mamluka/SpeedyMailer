using Ninject;
using Ninject.Modules;
using RestSharp;
using Ninject.Extensions.Conventions;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Master.Web.UI.Communication;
using SpeedyMailer.Master.Web.UI.Configurations;
using SpeedyMailer.Master.Web.UI.Mail;

namespace SpeedyMailer.Master.Web.UI.Bootstrappers
{
    public static class NinjectBootstrapper
    {
        public static IKernel Kernel { get; private set; }

        static NinjectBootstrapper()
        {
           
                Kernel = new StandardKernel();
                Kernel.Scan(x=>
                                {
                                    x.FromAssembliesMatching("SpeedyMailer.*");
                                    x.BindWith<DefaultBindingGenerator>();
                                    
                                });
                //Kernel.Load<MailDroneStandardModule>();
           

        }
    }

    class MailDroneStandardModule : NinjectModule 
    {
        public override void Load()
        {
            Kernel.Bind<IDroneCommunicationService>().To<DroneCommunicationService>();
            Kernel.Bind<IDroneMailOporations>().To<DroneMailOporations>();
            Kernel.Bind<IMailSender>().To<MailSender>();
            Kernel.Bind<IRestClient>().To<RestClient>();
            Kernel.Bind<IDroneConfigurationManager>().To<DroneConfigurationManager>();
            Kernel.Bind<IMailParser>().To<MailParser>();
            Kernel.Bind<IEmailSourceWeaver>().To<EmailSourceWeaver>();
            Kernel.Bind<IUrlCreator>().To<UrlCreator>();
   






        }
    }
}
