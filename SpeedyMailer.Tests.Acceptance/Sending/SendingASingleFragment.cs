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
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Tests.Acceptance.Sending
{
	[TestFixture]
	public class SendingASingleFragment : IntegrationTestBase
	{
		private Api _api;

		public override void ExtraSetup()
		{
			_api = MasterResolve<Api>();
		}

		[Test]
		public void WhenSendingTheCreativeShouldSendTheFragments()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			CreateTemplate();
			CreateList("my list");
			AddContactsToList("my list");
			SaveCreative();

			DroneActions.Initialize("http://localhost:5487", DefaultBaseUrl);
			DroneActions.Start();
		}

		private void SaveCreative()
		{
			var lists = _api.Call<ServiceEndpoints.GetLists, List<ListDescriptor>>();
			var templates = _api.Call<ServiceEndpoints.GetTemplates, List<Template>>(x => x.Type = TemplateType.Unsubscribe);

			_api.Call<ServiceEndpoints.SaveCreative>(x =>
														 {
															 x.ListId = lists[0].Id;
															 x.UnsubscribeTemplateId = templates[0].Id;
															 x.Body = CreateBodyWithLink("http://www.dealexpress.com/deal");
															 x.Subject = "hello world subject";
														 });
		}

		private string CreateBodyWithLink(string link)
		{
			return string.Format(@"<html><body>this email has a link inside of it <a href="" {0} "" >test link</as>""</body></html>", link);
		}

		private void AddContactsToList(string listName)
		{
			var lists = _api.Call<ServiceEndpoints.GetLists, List<ListDescriptor>>();

			var fileName = CsvTestingExtentions.GenerateFileName("sample");
			var csvRows = Fixture
				.CreateMany<ContactsListCsvRow>(1000)
				.ToList();

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
