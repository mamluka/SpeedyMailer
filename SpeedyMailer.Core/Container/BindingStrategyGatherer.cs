using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class BindingStrategyGatherer
    {
        private readonly ContainerStrapperOptions _options;

        public BindingStrategyGatherer(ContainerStrapperOptions options)
        {
            _options = options;
        }

        public DatabaseBindingGatherer BindInterfaceToDefaultImplementation()
        {
            _options.BindingStratery = BindingStrategy.BindInterfaceToDefaultImplementation;
            return new DatabaseBindingGatherer(_options);
        }

        
    }
}