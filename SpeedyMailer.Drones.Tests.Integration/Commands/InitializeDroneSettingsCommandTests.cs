using System.Net;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class InitializeDroneSettingsCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenCalledWithServiceBaseUrl_ShouldInitializeTheJsonSettings()
		{
			Api.PrepareApiResponse<ServiceEndpoints.Admin.GetRemoteServiceConfiguration, ServiceEndpoints.Admin.GetRemoteServiceConfiguration.Response>(
				x => x.ServiceBaseUrl = DefaultBaseUrl
				);

			DroneActions.ExecuteCommand<InitializeDroneSettingsCommand>(x => x.RemoteConfigurationServiceBaseUrl = DefaultBaseUrl);

			var apiCallSettings = DroneResolve<ApiCallsSettings>();
			var droneSettings = DroneResolve<DroneSettings>();

			apiCallSettings.ApiBaseUri.Should().Be(DefaultBaseUrl);
			droneSettings.BaseUrl.Should().Be(string.Format("http://{0}", GetLocalHost()));
			droneSettings.Identifier.Should().Be(GetLocalHost());
			droneSettings.Ip.Should().Be(GetIp());
		}

		private string GetIp()
		{
			var restClient = MasterResolve<IRestClient>();
			restClient.BaseUrl = "http://ipecho.net";
			var ip = restClient.Execute(new RestRequest("/plain"));

			return ip.Content;
		}

		private string GetLocalHost()
		{
			return Dns.GetHostEntry(Dns.GetHostName()).HostName;
		}
	}
}
