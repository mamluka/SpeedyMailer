using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;

namespace SpeedyMailer.EmailPool.MailDrone.Bootstrappers
{
    public static class NinjectBootstrapper
    {
        public static IKernel Kernel { get; private set; }

        static NinjectBootstrapper()
        {
           
                Kernel = new StandardKernel();
                Kernel.Load<MailDroneStandardModule>();
           

        }
    }

    class MailDroneStandardModule : NinjectModule 
    {
        public override void Load()
        {
            
        }
    }
}
