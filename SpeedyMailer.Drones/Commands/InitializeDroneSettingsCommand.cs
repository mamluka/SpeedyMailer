using System;
using System.Net;
using RestSharp;
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
		private readonly IRestClient _restClient;

		public InitializeDroneSettingsCommand(Api api, Framework framework,IRestClient restClient)
		{
			_restClient = restClient;
			_framework = framework;
			_api = api;
		}

		public override void Execute()
		{
			var response = _api.SetBaseUrl(RemoteConfigurationServiceBaseUrl).Call<ServiceEndpoints.Admin.GetRemoteServiceConfiguration, ServiceEndpoints.Admin.GetRemoteServiceConfiguration.Response>();

			_framework.EditJsonSettings<ApiCallsSettings>(x => x.ApiBaseUri = response.ServiceBaseUrl);
			_framework.EditJsonSettings<DroneSettings>(x =>
				{
					x.BaseUrl = string.Format("http://{0}:4253", GetDomain());
					x.Domain = GetDomain();
					x.Identifier = GetDomain();
					x.Ip = GetIp();

				});
			
			_framework.EditJsonSettings<EmailingSettings>(x =>
				                                              {
					                                              x.MailingDomain = GetDomain();
				                                              });
		}
		private string GetIp()
		{
			_restClient.BaseUrl = "http://ipecho.net";
			var ip = _restClient.Execute(new RestRequest("/plain"));

			return ip.Content;
		}

		private string GetDomain()
		{
			try
			{
				var proc = new System.Diagnostics.Process
				{
					EnableRaisingEvents = false,
					StartInfo =
					{
						FileName = "/bin/hostname",
						Arguments = "-d",
						RedirectStandardOutput = true,
						UseShellExecute = false
					}
				};

				proc.Start();
				proc.WaitForExit();

				return proc.StandardOutput.ReadLine().Trim();
			}
			catch (Exception)
			{
				return Dns.GetHostEntry(Dns.GetHostName()).HostName;
			}
			
		}
	}
}