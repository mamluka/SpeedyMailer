﻿using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class AdminModuleTests:IntegrationTestBase
	{
		[Test]
		public void FetchRemoteServiceSettings_WhenCalled_ShouldReturnTheBaseUrl()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			var api = DroneResolve<Api>();

			var result = api.Call<ServiceEndpoints.Admin.GetRemoteServiceSettings, ServiceEndpoints.Admin.GetRemoteServiceSettings.Response>();

			result.ServiceBaseUrl.Should().Be(DefaultBaseUrl);
		}
	}
}
