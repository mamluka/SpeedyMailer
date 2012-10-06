using System.Net;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class InitializeDroneSettingsCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenCalledWithServiceBaseUrl_ShouldInitializeTheJsonSettings()
		{
			PrepareApiResponse<ServiceEndpoints.Admin.GetRemoteServiceSettings, ServiceEndpoints.Admin.GetRemoteServiceSettings.Response>(
				x => x.ServiceBaseUrl = DefaultBaseUrl
				);

			DroneActions.ExecuteCommand<InitializeDroneSettingsCommand>(x=> x.RemoteConfigurationServiceBaseUrl = DefaultBaseUrl);

			var apiCallSettings = DroneResolve<ApiCallsSettings>();
			var droneSettings = DroneResolve<DroneSettings>();

			apiCallSettings.ApiBaseUri.Should().Be(DefaultBaseUrl);
			droneSettings.BaseUrl.Should().Be(string.Format("http://{0}:4253",GetLocalHost()));
			droneSettings.Identifier.Should().Be(GetLocalHost());
		}

		private string GetLocalHost()
		{
			return Dns.GetHostEntry(Dns.GetHostName()).HostName;
		}
	}
}
