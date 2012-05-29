using Ninject;

namespace SpeedyMailer.Drone.Bootstrappers
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