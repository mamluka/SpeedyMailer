using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using RestSharp;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.MailDrone.Communication;
using SpeedyMailer.EmailPool.MailDrone.Configurations;
using SpeedyMailer.EmailPool.MailDrone.Mail;
using Ninject.Extensions.Conventions;
namespace SpeedyMailer.EmailPool.MailDrone.Bootstrappers
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
        }
    }


}
