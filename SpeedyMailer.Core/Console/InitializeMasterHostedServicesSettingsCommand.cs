using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;

namespace SpeedyMailer.Core.Console
{
	public class InitializeMasterHostedServicesSettingsCommand:Command
	{
		public string BaseUrl { get; set; }
		public string DatabaseUrl { get; set; }

		private readonly Framework _framework;

		public InitializeMasterHostedServicesSettingsCommand(Framework framework)
		{
			_framework = framework;
		}

		public override void Execute()
		{
			_framework.EditStoreSettings<ServiceSettings>(x=> x.BaseUrl = BaseUrl);
		}
	}
}