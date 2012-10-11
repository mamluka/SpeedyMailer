using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class DealsModuleTests : IntegrationTestBase
	{

		[Test]
		public void Deals_WhenEndpointIsCalled_ShouldParseTheCreativeIdAndSendToTheUrl()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			ServiceActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "my List");

			var contact = new Contact { Email = "david@test.com" };

			ServiceActions.ExecuteCommand<AddContactsCommand, long>(x => x.Contacts = new[] { contact });

			var creative = new Creative
			{
				DealUrl = "http://m.microsoft.com/en-us/default.mspx",
			};

			Store.Store(creative);

			var urlData = IntergrationHelpers.Encode(new DealUrlData
														 {
															 ContactId = contact.Id,
															 CreativeId = creative.Id
														 });

			var result = CallEndpoint("/deals/" + urlData);

			result.ResponseUri.Should().Be(creative.DealUrl);
			result.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Test]
		public void Deals_WhenEndpointIsCalled_ShouldSaveTheClickForTheContact()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			ServiceActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "my List");

			var contact = new Contact { Email = "david@test.com" };

			ServiceActions.ExecuteCommand<AddContactsCommand, long>(x => x.Contacts = new[] { contact });

			var creative = new Creative
			{
				DealUrl = "http://m.microsoft.com/en-us/default.mspx",
			};

			Store.Store(creative);

			var urlData = IntergrationHelpers.Encode(new DealUrlData
														 {
															 ContactId = contact.Id,
															 CreativeId = creative.Id
														 });

			CallEndpoint("/deals/" + urlData);

			Store.WaitForEntitiesToExist<ContactActions>(1);

			var result = Store.Query<ContactActions>().First();

			result.Clicks.Should().Contain(x => x == creative.Id);
			result.ContactId.Should().Be(contact.Id);
		}
		
		[Test]
		public void Deals_WhenEndpointIsCalledAndContactActionAlradyExistForThisContact_ShouldSaveTheClickForTheContact()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			ServiceActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "my List");

			var contact = new Contact { Email = "david@test.com" };

			ServiceActions.ExecuteCommand<AddContactsCommand, long>(x => x.Contacts = new[] { contact });

			var creative = new Creative
			{
				DealUrl = "http://m.microsoft.com/en-us/default.mspx",
			};

			Store.Store(creative);

			Store.Store(new ContactActions
				            {
					            ContactId = contact.Id,
								Clicks = new List<string> { "creative/99"}
				            });

			var urlData = IntergrationHelpers.Encode(new DealUrlData
														 {
															 ContactId = contact.Id,
															 CreativeId = creative.Id
														 });

			CallEndpoint("/deals/" + urlData);

			Store.WaitForEntitiesToExist<ContactActions>(1);

			var result = Store.Query<ContactActions>().First();

			result.Clicks.Should().Contain(x => x == creative.Id);
			result.Clicks.Should().Contain(x => x == "creative/99");
			result.ContactId.Should().Be(contact.Id);
		}

		[Test]
		public void Deals_WhenTheEndpointIsHitWithoutUrlObject_ShouldRedirectToSomeDealInTheDatabase()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creative = new Creative
							   {
								   DealUrl = "http://m.microsoft.com/en-us/default.mspx"
							   };

			Store.Store(creative);

			var result = CallEndpoint("/deals");

			result.ResponseUri.Should().Be(creative.DealUrl);
		}

		[Test]
		public void Deals_WhenTheEndpointIsHitWithoutUrlObjectAndThereAreNoCreatives_ShouldReturnNotFound()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var result = CallEndpoint("/deals");

			result.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		private IRestResponse CallEndpoint(string resource)
		{
			var client = MasterResolve<IRestClient>();

			var request = new RestRequest(resource);
			client.BaseUrl = DefaultBaseUrl;

			var result = client.Execute(request);
			return result;
		}
	}
}
