using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;

namespace SpeedyMailer.Master.Service.Commands
{
	public class InitializeServiceSettingsCommand:Command
	{
		public string BaseUrl { get; set; }
		public string DatabaseUrl { get; set; }

		private readonly Framework _framework;

		public InitializeServiceSettingsCommand(Framework framework)
		{
			_framework = framework;
		}

		public override void Execute()
		{
			_framework.EditStoreSettings<ServiceSettings>(x=> x.BaseUrl = BaseUrl);
		}
	}
}