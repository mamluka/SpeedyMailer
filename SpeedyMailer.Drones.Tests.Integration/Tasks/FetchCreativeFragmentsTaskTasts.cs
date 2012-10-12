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
		public FetchCreativeFragmentsTaskTasts()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenStarted_ShouldFetchACreativeFragmentFromServer()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			Api.ListenToApiCall<ServiceEndpoints.Creative.FetchFragment>();

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			Api.AssertApiCalled<ServiceEndpoints.Creative.FetchFragment>();
		}

		[Test]
		public void Execute_WhenNotAllCreativePackagesAreSent_ShouldNotFetchANewFragment()
		{
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			Api.ListenToApiCall<ServiceEndpoints.Creative.FetchFragment>();

			var creativePackage = new CreativePackage
									  {
										  Id = "package/1"
									  };

			DroneActions.Store(creativePackage);

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			Api.AssertApiWasntCalled<ServiceEndpoints.Creative.FetchFragment>();
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldStartSendTheEmail()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			DroneActions.EditSettings<DroneSettings>(x => x.Identifier = "192.1.1.1");

			var recipients = new List<Recipient> { AddRecipient("contacts/1", "test@test.com") };

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x =>
																							  {
																								  x.Id = "fragment/1";
																								  x.CreativeId = "creative/1";
																								  x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
																								  x.Subject = "hello world subject";
																								  x.UnsubscribeTemplate = "here  is a template ^url^";
																								  x.Recipients = recipients;
																								  x.Service = new Service
																								  {
																									  BaseUrl = "http://www.topemail.com",
																									  DealsEndpoint = "deal",
																									  UnsubscribeEndpoint = "unsubscribe"
																								  };
																							  });

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			Email.AssertEmailSent(x => x.To.Any(email => email.Address == "test@test.com"));
			Email.AssertEmailSent(x => x.Subject == "hello world subject");
			Email.AssertEmailSent(x => x.DroneId == "192.1.1.1");
		}

		[Test]
		public void Execute_WhenRecipientsHaveIntervals_ShouldSendTheEmailsAccordingToTheIntervals()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);
			DroneActions.EditSettings<DroneSettings>(x => x.Identifier = "192.1.1.1");

			var recipients = new List<Recipient>
				                 {
					                 AddRecipient("contacts/1", "test@gmail.com"),
					                 AddRecipient("contacts/2", "test2@gmail.com"),
					                 AddRecipient("contacts/3", "test3@gmail.com"),
					                 AddRecipient("contacts/4", "test4@gmail.com"),
					                 AddRecipient("contacts/5", "test5@gmail.com"),
				                 };

			recipients.ForEach(x => x.Interval = 3);

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x =>
																							  {
																								  x.Id = "fragment/1";
																								  x.CreativeId = "creative/1";
																								  x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
																								  x.Subject = "hello world subject";
																								  x.UnsubscribeTemplate = "here  is a template ^url^";
																								  x.Recipients = recipients;
																								  x.Service = new Service
																												  {
																													  BaseUrl = "http://www.topemail.com",
																													  DealsEndpoint = "deal",
																													  UnsubscribeEndpoint = "unsubscribe"
																												  };
																							  });

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			Email.AssertEmailsSentWithInterval(recipients, 3);
		}

		[Test]
		public void Execute_WhenThereAreNoFragments_ShouldDoNothing()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(default(CreativeFragment));

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			Email.AssertEmailNotSent();
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldReplaceTheDealLinksWithRediractableServiceLinks()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			var recipients = new List<Recipient> { AddRecipient("contacts/1", "test@test.com") };

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x =>
																							  {
																								  x.Id = "fragment/1";
																								  x.CreativeId = "creative/1";
																								  x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
																								  x.Subject = "hello world subject";
																								  x.UnsubscribeTemplate = "here  is a template ^url^";
																								  x.Recipients = recipients;
																								  x.Service = new Service
																												  {
																													  BaseUrl = "http://www.topemail.com",
																													  DealsEndpoint = "deal",
																													  UnsubscribeEndpoint = "unsubscribe"
																												  };
																							  });

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertBodyContains("http://www.topemail.com/deal/" + IntergrationHelpers.Encode(new DealUrlData
																			{
																				ContactId = "contacts/1",
																				CreativeId = "creative/1"
																			}));
		}

		[Test]
		public void Execute_WhenBodyContainsEmailTemplatingElement_ShouldReplaceWithTheRecipientEmail()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			var recipients = new List<Recipient> { AddRecipient("contacts/1", "test@test.com") };

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x =>
																								  {
																									  x.Id = "fragment/1";
																									  x.CreativeId = "creative/1";
																									  x.Body = CreateBodyWithLinkAndEmailTemplating("http://www.dealexpress.com/deal");
																									  x.Subject = "hello world subject";
																									  x.UnsubscribeTemplate = "here  is a template ^url^";
																									  x.Recipients = recipients;
																									  x.Service = new Service
																													  {
																														  BaseUrl = "http://www.topemail.com",
																														  DealsEndpoint = "deal",
																														  UnsubscribeEndpoint = "unsubscribe"
																													  };
																								  });

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertBodyContains("also we have the ema1il here test@test.com");
		}

		private string CreateBodyWithLinkAndEmailTemplating(string link)
		{
			return CreateBodyWithLink(link) + " also we have the email here ^email^";
		}

		[Test]
		public void Execute_WhenWeObtainAFragment_ShouldAppendTheUnsubscribeTemplate()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);
			DroneActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

			var recipients = new List<Recipient> { AddRecipient("contacts/1", "test@test.com") };

			Api.PrepareApiResponse<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x =>
																							  {
																								  x.Id = "fragment/1";
																								  x.CreativeId = "creative/1";
																								  x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
																								  x.Subject = "hello world subject";
																								  x.UnsubscribeTemplate = "here  is a template ^url^";
																								  x.Recipients = recipients;
																								  x.Service = new Service
																												  {
																													  BaseUrl = "http://www.topemail.com",
																													  DealsEndpoint = "deal",
																													  UnsubscribeEndpoint = "unsubscribe"
																												  };
																							  });

			var task = new FetchCreativeFragmentsTask();

			DroneActions.StartScheduledTask(task);

			AssertBodyContains("here  is a template");
			AssertBodyContains("http://www.topemail.com/unsubscribe/" + IntergrationHelpers.Encode(new UnsubscribeUrlData
										{
											CreativeId = "creative/1",
											ContactId = "contacts/1"
										}));
		}



		private void AssertBodyContains(string text)
		{
			Email.AssertEmailSent(x => x.Body.Should().Contain(text));
		}

		private string CreateBodyWithLink(string link)
		{
			return string.Format(@"<html><body>this email has a link inside of it <a href="" {0} "" >test link</as>""</body></html>", link);
		}

		private static Recipient AddRecipient(string contactId, string email)
		{
			return new Recipient
					   {
						   Email = email,
						   ContactId = contactId,
						   Interval = 1
					   };
		}
	}

	public class UnsubscribeUrlData
	{
		public string CreativeId { get; set; }
		public string ContactId { get; set; }
	}
}
