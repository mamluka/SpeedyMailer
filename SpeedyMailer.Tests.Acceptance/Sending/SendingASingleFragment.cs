using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Tests.Acceptance.Sending
{
	[TestFixture]
	public class SendingASingleFragment : IntegrationTestBase
	{
		private Api _api;

		[Test]
		public void WhenSendingTheCreativeShouldSendTheFragments()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);

			_api = MasterResolve<Api>();

			ServiceActions.Initialize();
			ServiceActions.Start();

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.With(x => x.Email, "email" + Guid.NewGuid() + "@domain.com")
				.CreateMany(20)
				.ToList();

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();
			SendCreative(creativeId);

			WaitForEntitiesToExist<CreativeFragment>(30);

			var drone = DroneActions.CreateDrone("drone1", GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone.Initialize();
			drone.Start();

			var contacts = csvRows.Select(x => x.Email);
			AssertEmailsSentTo(contacts);

		}

		[Test]
		public void SendingUsingTwoDrones()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => { x.RecipientsPerFragment = 50; });

			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = AssemblyDirectory);

			_api = MasterResolve<Api>();

			ServiceActions.Initialize();
			ServiceActions.Start();

			var csvRows = Fixture
				.Build<ContactsListCsvRow>()
				.With(x => x.Email, "email" + Guid.NewGuid() + "@domain.com")
				.CreateMany(200)
				.ToList();

			CreateTemplate();
			CreateList("my list");

			AddContactsToList("my list", csvRows);
			var creativeId = SaveCreative();
			SendCreative(creativeId);

			var drone1 = DroneActions.CreateDrone("drone1", GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone1.Initialize();
			drone1.Start();

			var drone2 = DroneActions.CreateDrone("drone2", GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone2.Initialize();
			drone2.Start();

			AssertEmailsSentBy("drone1", 100, 120);
			AssertEmailsSentBy("drone1", 100, 120);
		}

		private void SendCreative(string creativeId)
		{
			_api.Call<ServiceEndpoints.Send>(x => x.CreativeId = creativeId);
		}

		private string SaveCreative()
		{
			var lists = _api.Call<ServiceEndpoints.GetLists, List<ListDescriptor>>();
			var templates = _api.Call<ServiceEndpoints.GetTemplates, List<Template>>(x => x.Type = TemplateType.Unsubscribe);

			var result = _api.Call<ServiceEndpoints.SaveCreative, ApiStringResult>(x =>
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
			var lists = _api.Call<ServiceEndpoints.GetLists, List<ListDescriptor>>();

			var fileName = CsvTestingExtentions.GenerateFileName("sample");

			csvRows.ToCsvFile(fileName);

			_api.AddFiles(new[] { fileName }).Call<ServiceEndpoints.UploadContacts>(x =>
																					  {
																						  x.ListName = lists[0].Id;
																					  });

		}

		private void CreateList(string listName)
		{
			_api.Call<ServiceEndpoints.CreateList>(x =>
													   {
														   x.Name = listName;
													   });
		}

		private void CreateTemplate()
		{
			_api.Call<ServiceEndpoints.CreateUnsubscribeTemplate>(x =>
																	  {
																		  x.Body = "here is my template <url>";
																	  });
		}
	}
}
