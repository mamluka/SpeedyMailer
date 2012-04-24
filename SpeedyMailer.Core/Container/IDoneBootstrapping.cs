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
            return ContainerBootstrapper.Execute(_options);
        }
    }
}