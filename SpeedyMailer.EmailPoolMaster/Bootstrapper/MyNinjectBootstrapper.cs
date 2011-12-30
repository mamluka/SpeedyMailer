using Nancy.Bootstrappers.Ninject;
using Ninject;
using SpeedyMailer.EmailPoolMaster.Pool;

namespace SpeedyMailer.EmailPoolMaster.Bootstrapper
{
    public class MyNinjectBootstrapper:NinjectNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            
        }
    }
}