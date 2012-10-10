using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Tests.Acceptance.Sending
{
	[TestFixture]
	public class SendingEmails : IntegrationTestBase
	{
		private Api _api;

		[Test]
		public void SendingEmailsWithOneDrone()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			_api = MasterResolve<Api>();

			ServiceActions.Initialize();
			ServiceActions.Start();

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.Without(x => x.Email)
				.CreateMany(10)
				.ToList();

			csvRows.ForEach(x => x.Email = "email" + Guid.NewGuid() + "@domain.com");

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();
			SendCreative(creativeId);

			var drone = DroneActions.CreateDrone("drone1", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone.Initialize();
			drone.Start();

			var contacts = csvRows.Select(x => x.Email).ToList();
			Email.AssertEmailsSentWithInterval(contacts, 1);
		}

		[Test]
		public void SendingEmailsUsingTwoDrones()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => { x.RecipientsPerFragment = 10; });

			_api = MasterResolve<Api>();

			ServiceActions.Initialize();
			ServiceActions.Start();

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.CreateMany(40)
				.ToList();

			csvRows.ForEach(x => x.Email = "email" + Guid.NewGuid() + "@domain.com");

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();
			SendCreative(creativeId);

			var drone1 = DroneActions.CreateDrone("drone1", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone1.Initialize();
			drone1.Start();

//			var drone2 = DroneActions.CreateDrone("drone2", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
//			drone2.Initialize();
//			drone2.Start();

			Email.AssertEmailsSentBy("drone1", 40,60);
//			Email.AssertEmailsSentBy("drone2", 20);
		}

		[Test]
		public void SendingEmailsUsingFiveDrones()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => { x.RecipientsPerFragment = 50; });

			_api = MasterResolve<Api>();

			ServiceActions.Initialize();
			ServiceActions.Start();

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.With(x => x.Email, "email" + Guid.NewGuid() + "@domain.com")
				.CreateMany(500)
				.ToList();

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();
			SendCreative(creativeId);

			var drone1 = DroneActions.CreateDrone("drone1", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone1.Initialize();
			drone1.Start();

			var drone2 = DroneActions.CreateDrone("drone2", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone2.Initialize();
			drone2.Start();

			var drone3 = DroneActions.CreateDrone("drone3", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone3.Initialize();
			drone3.Start();

			var drone4 = DroneActions.CreateDrone("drone4", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone4.Initialize();
			drone4.Start();

			var drone5 = DroneActions.CreateDrone("drone5", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone5.Initialize();
			drone5.Start();

			Email.AssertEmailsSentBy("drone1", 100, 150);
			Email.AssertEmailsSentBy("drone2", 100, 150);
			Email.AssertEmailsSentBy("drone3", 100, 150);
			Email.AssertEmailsSentBy("drone4", 100, 150);
			Email.AssertEmailsSentBy("drone5", 100, 150);
		}

		private void SendCreative(string creativeId)
		{
			_api.Call<ServiceEndpoints.Creative.Send>(x => x.CreativeId = creativeId);
		}

		private string SaveCreative()
		{
			var lists = _api.Call<ServiceEndpoints.Lists.GetLists, List<ListDescriptor>>();
			var templates = _api.Call<ServiceEndpoints.Templates.GetTemplates, List<Template>>(x => x.Type = TemplateType.Unsubscribe);

			var result = _api.Call<ServiceEndpoints.Creative.SaveCreative, ApiStringResult>(x =>
														 {
															 x.ListId = lists[0].Id;
															 x.UnsubscribeTemplateId = templates[0].Id;
															 x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
															 x.Subject = "hello world subject";
														 });

			return result.Result;
		}

		private string CreateBodyWithLink(string link)
		{
			return string.Format(@"<html><body>this email has a link inside of it <a href="" {0} "" >test link</as>""</body></html>", link);
		}

		private void AddContactsToList(string listName, IEnumerable<ContactsListCsvRow> csvRows)
		{
			var lists = _api.Call<ServiceEndpoints.Lists.GetLists, List<ListDescriptor>>();

			var fileName = CsvTestingExtentions.GenerateFileName("sample");

			csvRows.ToCsvFile(fileName);

			_api.AddFiles(new[] { fileName }).Call<ServiceEndpoints.Lists.UploadContacts>(x =>
																					  {
																						  x.ListName = lists[0].Id;
																					  });

		}

		private void CreateList(string listName)
		{
			_api.Call<ServiceEndpoints.Lists.CreateList>(x =>
													   {
														   x.Name = listName;
													   });
		}

		private void CreateTemplate()
		{
			_api.Call<ServiceEndpoints.Templates.CreateUnsubscribeTemplate>(x =>
																	  {
																		  x.Body = "here is my template <url>";
																	  });
		}
	}
}
