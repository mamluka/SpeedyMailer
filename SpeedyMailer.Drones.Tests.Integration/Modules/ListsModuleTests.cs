using System;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Modules
{
	public class ListsModuleTests : IntegrationTestBase
	{
		public ListsModuleTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Unsubscribe_WhenCalled_ShouldSaveTheUnsubscribeRequest()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.BaseUrl = DefaultBaseUrl;
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.StartDroneEndpoints();

			var result = MakeUnsubscribeRequest();

			result.Content.Should().Be("You are now unsubscribed, have a nice day");
		}

		[Test]
		public void Unsubscribe_WhenCalled_ShouldSaveTheCickAction()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
			{
				x.BaseUrl = DefaultBaseUrl;
				x.StoreHostname = DefaultHostUrl;
			});

			DroneActions.StartDroneEndpoints();

			MakeUnsubscribeRequest();

			DroneActions.WaitForDocumentToExist<UnsubscribeRequest>();

			var result = DroneActions.FindSingle<UnsubscribeRequest>();

			result.ContactId.Should().Be("contacts/1");
			result.CreativeId.Should().Be("creative/1");
			result.Date.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
		}

		private IRestResponse MakeUnsubscribeRequest()
		{
			var restClient = new RestClient();
			var restRequest = CreateRestRequest();
			var result = restClient.Execute(restRequest);
			return result;
		}

		private RestRequest CreateRestRequest()
		{
			var resource = DefaultBaseUrl + "/unsubscribe/" + UrlBuilder.ToBase64(new DealUrlData
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
