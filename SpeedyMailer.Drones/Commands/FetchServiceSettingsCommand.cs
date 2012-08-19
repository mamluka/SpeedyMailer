using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class FetchServiceSettingsCommand : Command<RemoteServiceSettings>
	{
		private readonly Api _api;

		public FetchServiceSettingsCommand(Api api)
		{
			_api = api;
		}

		public override RemoteServiceSettings Execute()
		{
			var response = _api.Call<ServiceEndpoints.FetchServiceSettings, ServiceEndpoints.FetchServiceSettings.Response>();
			return new RemoteServiceSettings
			       	{
			       		BaseUrl = response.ServiceBaseUrl
			       	};
		}
	}
}