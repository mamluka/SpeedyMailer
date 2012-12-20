using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Logging;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.General
{
	public class MongoLoggingTargetTests : IntegrationTestBase
	{
		public MongoLoggingTargetTests()
			: base(x =>
				{
					x.UseMongo = true;
					x.UseDefaultMongoPort = true;
				})
		{ }

		[Test]
		public void Exception_WhenExptionHappend_ShouldLogIt()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
				{
					x.BaseUrl = DefaultBaseUrl;
					x.StoreHostname = DefaultHostUrl;
				});

			DroneActions.StartDroneEndpoints();

			CreateNancyException();

			var result = DroneActions.FindAll<DroneException>();

			result.Should().HaveCount(1);
		}

		private void CreateNancyException()
		{
			var restClient = new RestClient();
			var restRequest = CreateRestRequest();
			restClient.Execute(restRequest);
		}

		private RestRequest CreateRestRequest()
		{
			var resource = DefaultBaseUrl + "/admin/exception";

			var restRequest = new RestRequest(resource)
			{
				JsonSerializer = new RestSharpJsonNetSerializer(),
				Method = Method.GET
			};

			return restRequest;
		}
	}
}
