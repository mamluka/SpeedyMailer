using System.Net;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class InitializeDroneSettingsCommand : Command
	{
		public string RemoteConfigurationServiceBaseUrl { get; set; }

		private readonly Api _api;
		private readonly Framework _framework;

		public InitializeDroneSettingsCommand(Api api, Framework framework)
		{
			_framework = framework;
			_api = api;
		}

		public override void Execute()
		{
			var response = _api.SetBaseUrl(RemoteConfigurationServiceBaseUrl).Call<ServiceEndpoints.Admin.GetRemoteServiceSettings, ServiceEndpoints.Admin.GetRemoteServiceSettings.Response>();

			_framework.EditJsonSettings<ApiCallsSettings>(x => x.ApiBaseUri = response.ServiceBaseUrl);
			_framework.EditJsonSettings<DroneSettings>(x =>
				{
					x.BaseUrl = string.Format("http://{0}:4253", GetLocalHost());
					x.Identifier = GetLocalHost();

				});
		}

		private string GetLocalHost()
		{
			return Dns.GetHostEntry(Dns.GetHostName()).HostName;
		}
	}
}