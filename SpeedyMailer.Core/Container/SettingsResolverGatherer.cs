using System;
using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class SettingsResolverGatherer:IDoneBootstrapping
    {
        private readonly ContainerStrapperOptions _options;

        public SettingsResolverGatherer(ContainerStrapperOptions options)
        {
            _options = options;
        }

		public IDoneBootstrapping Settings(Action<SettingsResolverGathererOptions> settingsAction)
        {
            var settingsResolverGathererOptions = new SettingsResolverGathererOptions(_options);
            settingsAction.Invoke(settingsResolverGathererOptions);
			return new DoneBootstrapping(_options);
        }

        public IKernel Done()
        {
            return ContainerBootstrapper.Execute(_options);
        }
    }
}