using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using FluentAssertions;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	[TestFixture]
	public class CreativeModuleTests : IntegrationTestBase
	{
		[Test]
		public void Add_WhenGivenACreativeId_ShouldCreateAnCreativeFragments()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative(1000);

			var api = MasterResolve<Api>();
			api.Call<CreativeEndpoint.Add>(x => x.CreativeId = creativeId);

			WaitForEntitiesToExist<CreativeFragment>(1);
			var result = Query<CreativeFragment>().First();

			result.Recipients.Should().HaveCount(100);
			result.CreativeId.Should().Be(creativeId);
		}

		[Test]
		public void Save_WhenCalledWithParameters_ShouldSaveTheCreative()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			var creativeId = api.Call<ServiceEndpoints.SaveCreative, ApiStringResult>(x =>
														{
															x.Body = "body";
															x.DealUrl = "dealUrl";
															x.ListId = "list/1";
															x.Subject = "subject";
															x.UnsubscribeTemplateId = "templateId";
														}).Result;

			WaitForEntityToExist(creativeId);

			var result = Load<Creative>(creativeId);

			result.Body.Should().Be("body");
			result.DealUrl.Should().Be("dealUrl");
			result.Lists.Should().Contain("list/1");
			result.Subject.Should().Be("subject");
			result.UnsubscribeTemplateId.Should().Be("templateId");
		}

		[Test]
		public void Fragments_WhenCalledByDrone_ShouldSendBackAFragment()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative(100);
			CreateFragment(creativeId, 100);

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.FetchFragment, CreativeFragment>();

			result.Body.Should().Be("body");
			result.CreativeId.Should().Be(creativeId);
			result.Recipients.Should().HaveCount(100);
			result.Service.BaseUrl.Should().Be(DefaultBaseUrl);
			result.Service.DealsEndpoint.Should().Be("deals");
			result.Service.UnsubscribeEndpoint.Should().Be("lists/unsubscribe");
		}

		[Test]
		public void Fragments_WhenCalledCuncurrently_ShouldNotGiveTheSameFragmentToTwoDrones()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative(100);
			CreateFragment(creativeId, 10);

			var fragmentList = new List<string>();

			var drone1 = new Thread(x => Enumerable.Range(1, 50).ToList().ForEach(i =>
																					  {
																						  var api = MasterResolve<Api>();
																						  var result = api.Call<ServiceEndpoints.FetchFragment, CreativeFragment>();

																						  if (result == null)
																							  return;

																						  fragmentList.Add(result.Id);
																					  }));

			var drone2 = new Thread(x => Enumerable.Range(1, 50).ToList().ForEach(i =>
																					  {
																						  var api = MasterResolve<Api>();
																						  var result = api.Call<ServiceEndpoints.FetchFragment, CreativeFragment>();

																						  if (result == null)
																							  return;

																						  fragmentList.Add(result.Id);
																					  }));

			drone1.Start();
			drone2.Start();

			drone1.Join();
			drone2.Join();

			fragmentList.Should().OnlyHaveUniqueItems();
			fragmentList.Should().HaveCount(10);
		}

		private void CreateFragment(string creativeId, int recipientsPerFragment)
		{
			var taskId = ServiceActions.ExecuteTask(new CreateCreativeFragmentsTask
										   {
											   CreativeId = creativeId,
											   RecipientsPerFragment = recipientsPerFragment
										   });

			WaitForTaskToComplete(taskId);
		}

		private string CreateCreative(int contactsCount)
		{
			var listId = UIActions.CreateListWithRandomContacts("MyList", contactsCount);
			var unsubscribeTenplateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x => x.Body = "body");
			var creativeId = UIActions.CreateSimpleCreative(new[] { listId }, unsubscribeTenplateId);
			return creativeId;
		}
	}
}
