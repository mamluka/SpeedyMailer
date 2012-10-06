using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Integration.Utils;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	[TestFixture]
	public class ListsModuleTests:IntegrationTestBase
	{
		[Test]
		public void GetLists_WhenCalled_ShouldGetAllListIds()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var listId = ServiceActions.ExecuteCommand<CreateListCommand, string>(x => x.Name = "myList");

			var api = MasterResolve<Api>();

			var lists = api.Call<ServiceEndpoints.Lists.GetLists, List<ListDescriptor>>();

			lists.Should().Contain(x=> x.Id == listId && x.Name == "myList");
		}

		[Test]
		public void Create_WhenCalled_ShouldSaveANewListToTheDatabase()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.Lists.CreateList,ApiStringResult>(x=> x.Name = "myName");

			var list = Load<ListDescriptor>(result.Result);

			list.Name.Should().Be("myName");
		}

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
			api.AddFiles(new[] { fileName })
				.Call<ServiceEndpoints.Lists.UploadContacts>(x => x.ListName = "list/1");

			WaitForEntitiesToExist<Contact>(1000);

			var firstContact = list.First();
			var result = Query<Contact>(x => x.Name == firstContact.Name).First();

			result.MemberOf.Should().Contain("list/1");
			result.Email.Should().Be(firstContact.Email);
			result.Country.Should().Be(firstContact.Country);
		}

		[Test]
		public void Rules_WhenAddingARule_ShouldSaveTheRule()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Rules.Save>();

		}
	}
}
