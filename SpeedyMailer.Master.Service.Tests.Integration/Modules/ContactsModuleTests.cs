using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	public class ContactsModuleTests : IntegrationTestBase
	{
		[Test]
		public void UploadList_WhenCalled_ShouldSaveTheFile()
	    {
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

		    var fileName = CsvTestingExtentions.GenerateFileName("sample");
		    var list = Fixture.CreateMany<ContactsListCsvRow>(1000);
			list.ToCsvFile(fileName);

			var api = MasterResolve<Api>();
			api.AddFiles(fileName)
				.Call<ServiceEndpoints.UploadContacts>();


	    }


	}
}
