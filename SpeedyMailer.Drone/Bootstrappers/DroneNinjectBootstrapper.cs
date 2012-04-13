using Ninject;
using Ninject.Extensions.Conventions;

namespace SpeedyMailer.Master.Web.UI.Bootstrappers
{
    public static class DroneNinjectBootstrapper
    {
        static DroneNinjectBootstrapper()
        {
            Kernel = new StandardKernel();

           
        }

        public static IKernel Kernel { get; private set; }
    }
}