using Ninject;

namespace SpeedyMailer.Core.Container
{
    public interface IDoneBootstrapping
    {
        IKernel Done();
    }

    public class DoneBootstrapping : IDoneBootstrapping
    {
        private readonly ContainerStrapperOptions _options;

        public DoneBootstrapping(ContainerStrapperOptions options)
        {
            _options = options;
        }

        public IKernel Done()
        {
            if (ContainerBootstrapper.Kernel != null)
            {
                return ContainerBootstrapper.Execute(_options,ContainerBootstrapper.Kernel);
            }
            return ContainerBootstrapper.Execute(_options);
        }
    }
}