using System;
using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class DatabaseBindingGatherer:IDoneBootstrapping
    {
        private readonly ContainerStrapperOptions _options;

        public DatabaseBindingGatherer(ContainerStrapperOptions options)
        {
            _options = options;
        }

        public SettingsResolverGatherer Storage<T>(Action<DatabaseBindingGathererOptions<T>> storageAction)
        {
            var options = new DatabaseBindingGathererOptions<T>(_options);
            storageAction.Invoke(options);
            return new SettingsResolverGatherer(_options);
        }

        public IKernel Done()
        {
            return ContainerBootstrapper.Execute(_options);
        }


        public SettingsResolverGatherer NoDatabase()
        {
            _options.NoDatabase = true;
            return new SettingsResolverGatherer(_options);
        }
    }
}