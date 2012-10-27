using System.Net;
using System.Net.NetworkInformation;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;

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
			var response = _api.SetBaseUrl(RemoteConfigurationServiceBaseUrl).Call<ServiceEndpoints.Admin.GetRemoteServiceConfiguration, ServiceEndpoints.Admin.GetRemoteServiceConfiguration.Response>();

			_framework.EditJsonSettings<ApiCallsSettings>(x => x.ApiBaseUri = response.ServiceBaseUrl);
			_framework.EditJsonSettings<DroneSettings>(x =>
				{
					x.BaseUrl = string.Format("http://{0}:4253", GetLocalHost());
					x.Identifier = GetLocalHost();

				});
		}

		private string GetLocalHost()
		{
			var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
			return string.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
		}
	}
}