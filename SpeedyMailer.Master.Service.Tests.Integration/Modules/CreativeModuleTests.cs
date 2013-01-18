using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
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
		public void Send_WhenGivenACreativeId_ShouldCreateAnCreativeFragments()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative(1000);

			var api = MasterResolve<Api>();
			api.Call<ServiceEndpoints.Creative.Send>(x => x.CreativeId = creativeId);

			Store.WaitForEntitiesToExist<CreativeFragment>(1, 10);
			var result = Store.Query<CreativeFragment>().First();

			result.Recipients.Should().HaveCount(200);
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

			var creativeId = api.Call<ServiceEndpoints.Creative.SaveCreative, ApiStringResult>(x =>
														{
															x.HtmlBody = "body";
															x.TextBody = "text body";
															x.DealUrl = "dealUrl";
															x.ListId = "list/1";
															x.Subject = "subject";
															x.UnsubscribeTemplateId = "templateId";
															x.FromName = "david";
															x.FromAddressDomainPrefix = "sales";
														}).Result;

			Store.WaitForEntityToExist(creativeId);

			var result = Store.Load<Creative>(creativeId);

			result.HtmlBody.Should().Be("body");
			result.TextBody.Should().Be("text body");
			result.DealUrl.Should().Be("dealUrl");
			result.Lists.Should().Contain("list/1");
			result.Subject.Should().Be("subject");
			result.UnsubscribeTemplateId.Should().Be("templateId");
			result.FromName.Should().Be("david");
			result.FromAddressDomainPrefix.Should().Be("sales");
		}

		[Test]
		public void GetAll_WhenCalled_ShouldReturnTheCurrentlyStoredCreatives()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var api = MasterResolve<Api>();

			var creative1 = CreateCreative(100);
			var creative2 = CreateCreative(100);

			var result = api.Call<ServiceEndpoints.Creative.GetAll, List<Creative>>();

			result.Should().Contain(x => x.Id == creative1);
			result.Should().Contain(x => x.Id == creative2);
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

			var result = api.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

			result.HtmlBody.Should().Be("body");
			result.CreativeId.Should().Be(creativeId);
			result.Recipients.Should().HaveCount(100);
			result.FromName.Should().Be("david");
			result.FromAddressDomainPrefix.Should().Be("sales");
			result.DealUrl.Should().Be("dealUrl");
		}

		[Test]
		public void Fragments_WhenCalledByDrone_ShouldSaveDroneIdAndFetchTime()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });
			ServiceActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative(100);
			CreateFragment(creativeId, 100);

			var api = MasterResolve<Api>();

			api.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>(x => x.DroneId = "drone1.com");

			var result = Store.Query<CreativeFragment>().First();

			result.FetchedBy.Should().Be("drone1.com");
			result.FetchedAt.Should().BeAfter(DateTime.UtcNow.AddSeconds(-30));
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
																						  var result = api.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

																						  if (result == null)
																							  return;

																						  fragmentList.Add(result.Id);
																					  }));

			var drone2 = new Thread(x => Enumerable.Range(1, 50).ToList().ForEach(i =>
																					  {
																						  var api = MasterResolve<Api>();
																						  var result = api.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

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
			ServiceActions.EditSettings<CreativeFragmentSettings>(x => x.RecipientsPerFragment = recipientsPerFragment);
			;
			var taskId = ServiceActions.ExecuteTask(new CreateCreativeFragmentsTask
										   {
											   CreativeId = creativeId,
										   });

			Tasks.WaitForTaskToComplete(taskId);
		}

		private string CreateCreative(int contactsCount)
		{
			var listId = UiActions.CreateListWithRandomContacts("MyList", contactsCount);
			var unsubscribeTenplateId = UiActions.ExecuteCommand<CreateTemplateCommand, string>(x => x.Body = "body");
			var creativeId = UiActions.CreateSimpleCreative(new[] { listId }, unsubscribeTenplateId);
			return creativeId;
		}
	}
}
