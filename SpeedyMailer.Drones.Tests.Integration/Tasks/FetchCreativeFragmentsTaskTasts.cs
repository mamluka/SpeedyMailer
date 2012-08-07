using System.Collections.Generic;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;
using System.Linq;
using FluentAssertions;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class FetchCreativeFragmentsTaskTasts : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenStarted_ShouldFetchACreativeFragmentFromServer()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			ListenToApiCall<ServiceEndpoints.FetchFragment>();

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertApiCalled<ServiceEndpoints.FetchFragment>();
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldStartSendTheEmail()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			PrepareApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x =>
					x.CreativeFragment = CreateCreativeFragmet()
				);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertEmailSent(x => x.To.Any(email => email.Address == "test@test.com"));
			AssertEmailSent(x => x.Subject == "hello world subject");
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldReplaceTheDealLinksWithRediractableServiceLinks()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			PrepareApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x =>
					x.CreativeFragment = CreateCreativeFragmet()
				);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertBodyContains("http://www.topemail.com/deal/" + Encode(new DealUrlData
																			{
																				ContactId = "contact/1",
																				CreativeId = "creative/1"
																			}));
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldAppendTheUnsubscribeTemplate()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			PrepareApiResponse<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>(x =>
																										x.CreativeFragment = CreateCreativeFragmet()
				);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertBodyContains("here  is a template");
			AssertBodyContains("http://www.topemail.com/unsubscribe/"+ Encode(new UnsubscribeUrlData
			                          	{
			                          		CreativeId = "creative/1",
			                          		ContactId = "contact/1"
			                          	}));
		}

		private static string Encode(object obj)
		{
			return UrlBuilder.ToBase64(obj);
		}

		private void AssertBodyContains(string text)
		{
			AssertEmailSent(x => x.Body.Should().Contain(text));
		}

		private CreativeFragment CreateCreativeFragmet()
		{
			return new CreativeFragment
					{
						Id = "fragment/1",
						CreativeId = "creative/1",
						Body = CreateBodyWithLink("http://www.dealexpress.com/deal"),
						Subject = "hello world subject",
						UnsubscribeTemplate = "here  is a template <url>",
						Recipients = AddContact("contact/1", "test@test.com"),
						Service = new Service
									{
										BaseUrl = "http://www.topemail.com",
										DealsEndpoint = "deal",
										UnsubscribeEndpoint = "unsubscribe"
									}
					};
		}

		private string CreateBodyWithLink(string link)
		{
			return string.Format(@"<html><body>this email has a link inside of it <a href="" {0} "" >test link</as>""</body></html>", link);
		}

		private static List<Contact> AddContact(string contactId, string email)
		{
			return new List<Contact>
			       	{
			       		new Contact
			       			{
								Id = contactId,
			       				Email = email
			       			}
			       	};
		}
	}

	public class UnsubscribeUrlData
	{
		public string CreativeId { get; set; }
		public string ContactId { get; set; }
	}
}
