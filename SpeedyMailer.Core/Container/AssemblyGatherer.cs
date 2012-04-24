using System;
using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class AssemblyGatherer
    {
        private readonly ContainerStrapperOptions _options;

        public AssemblyGatherer(ContainerStrapperOptions options)
        {
            _options = options;
        }

        public BindingStrategyGatherer Analyze(Action<AssemblyGathererOptions> gatherAction)
        {
            var assemblyGathererOptions = new AssemblyGathererOptions(_options);
            gatherAction.Invoke(assemblyGathererOptions);
            return new BindingStrategyGatherer(_options);
        }

       
    }
}