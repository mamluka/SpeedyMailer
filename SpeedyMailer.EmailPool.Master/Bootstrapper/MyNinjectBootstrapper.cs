using Nancy.Bootstrappers.Ninject;
using Ninject;

namespace SpeedyMailer.EmailPool.Master.Bootstrapper
{
    public class MyNinjectBootstrapper:NinjectNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(IKernel existingContainer)
        {
            
        }
    }
}