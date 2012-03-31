using Ninject;
using Ninject.Extensions.Conventions;

namespace SpeedyMailer.Master.Web.UI.Bootstrappers
{
    public static class NinjectBootstrapper
    {
        static NinjectBootstrapper()
        {
            Kernel = new StandardKernel();

            Kernel.Bind(x =>
                        x.FromAssembliesMatching("SpeedyMailer.*")
                            .SelectAllClasses()
                            .BindDefaultInterface()
                );
        }

        public static IKernel Kernel { get; private set; }
    }
}