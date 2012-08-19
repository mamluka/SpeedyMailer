using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class FetchServiceSettingsCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenCalledWithServiceBaseUrl_ShouldFetchTheSettings()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			PrepareApiResponse<ServiceEndpoints.FetchServiceSettings, ServiceEndpoints.FetchServiceSettings.Response>(
				x => x.ServiceBaseUrl = DefaultBaseUrl
				);

			var result = DroneActions.ExecuteCommand<FetchServiceSettingsCommand, RemoteServiceSettings>();

			result.BaseUrl.Should().Be(DefaultBaseUrl);
		}
	}
}
