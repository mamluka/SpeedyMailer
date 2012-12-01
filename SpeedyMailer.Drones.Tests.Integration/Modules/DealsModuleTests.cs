﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Modules
{
	public class DealsModuleTests : IntegrationTestBase
	{
		public DealsModuleTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Deals_WhenAccessed_ShouldRedirectToTheDealsUrl()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.BaseUrl = DefaultBaseUrl;
															 x.StoreHostname = DefaultHostUrl;
														 });

			DroneActions.StartDroneEndpoints();

			DroneActions.Store(new CreativeToDealMap
								   {
									   Id = "creative/1",
									   DealUrl = "http://m.microsoft.com/en-us/default.mspx"
								   });

			var result = MakeDealRequest();

			result.ResponseUri.Should().Be("http://m.microsoft.com/en-us/default.mspx");
		}

		[Test]
		public void Deals_WhenAccessed_ShouldSaveTheCickAction()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.BaseUrl = DefaultBaseUrl;
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.StartDroneEndpoints();

			DroneActions.Store(new CreativeToDealMap
			{
				Id = "creative/1",
				DealUrl = "http://m.microsoft.com/en-us/default.mspx"
			});

			MakeDealRequest();

			DroneActions.WaitForDocumentToExist<ClickAction>();

			var result = DroneActions.FindSingle<ClickAction>();

			result.ContactId.Should().Be("contacts/1");
			result.CreativeId.Should().Be("creative/1");
			result.Date.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
		}

		private IRestResponse MakeDealRequest()
		{
			var restClient = new RestClient();
			var restRequest = CreateRestRequest();
			var result = restClient.Execute(restRequest);
			return result;
		}

		private RestRequest CreateRestRequest()
		{
			var resource = DefaultBaseUrl + "/deals/" + UrlBuilder.ToBase64(new DealUrlData
																				{
																					ContactId = "contacts/1",
																					CreativeId = "creative/1"
																				});

			var restRequest = new RestRequest(resource)
								  {
									  JsonSerializer = new RestSharpJsonNetSerializer(),
									  Method = Method.GET
								  };
			return restRequest;
		}
	}
}