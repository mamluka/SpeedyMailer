using Ninject.Extensions.Conventions.Syntax;

namespace SpeedyMailer.Core.Container
{
	public class ConfigurationsGatherer
	{
		private readonly ContainerStrapperOptions _options;

		public ConfigurationsGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public DatabaseBindingGatherer Configure(ConfigurationAction action)
		{
			_options.ConfigurationAction = action;
			return new DatabaseBindingGatherer(_options);
		}

		public DatabaseBindingGatherer DefaultConfiguration()
		{
			_options.ConfigurationAction = opt => opt.InSingletonScope();
			return new DatabaseBindingGatherer(_options);
		}
	}
}
