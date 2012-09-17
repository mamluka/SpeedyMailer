using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
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
		public void UploadList_WhenCalled_ShouldAddContactsToTheDatabase()
	    {
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

		    var fileName = CsvTestingExtentions.GenerateFileName("sample");
		    var list = Fixture
                .CreateMany<ContactsListCsvRow>(1000)
                .ToList();

			list.ToCsvFile(fileName);

			var api = MasterResolve<Api>();
			api.AddFiles(new[] {fileName})
				.Call<ServiceEndpoints.UploadContacts>(x=> x.ListName = "list/1");

            WaitForEntitiesToExist<Contact>(1000);

		    var firstContact = list.First();
		    var result = Query<Contact>(x => x.Name == firstContact.Name).First();

		    result.MemberOf.Should().Contain("list/1");
		    result.Email.Should().Be(firstContact.Email);
		    result.Country.Should().Be(firstContact.Country);
	    }
	}
}
