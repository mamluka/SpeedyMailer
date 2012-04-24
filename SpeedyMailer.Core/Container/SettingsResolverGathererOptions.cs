using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class SettingsResolverGathererOptions
    {
        private ContainerStrapperOptions _options;

        public SettingsResolverGathererOptions(ContainerStrapperOptions options)
        {
            _options = options;
        }

        public void UseDocumentDatabase()
        {
            _options.Settings = SettingsType.DocumentDatabase;
        }

        public void UseJsonFiles()
        {
            _options.Settings = SettingsType.JsonFiles;
        }
    }
}