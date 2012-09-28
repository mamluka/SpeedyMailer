using System.Linq;
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

			var creativeId = CreateCreative();

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

			var creativeId = CreateCreative();
			CreateFragment(creativeId);

			var api = MasterResolve<Api>();

			var result = api.Call<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>();

			result.CreativeFragment.Body.Should().Be("body");
			result.CreativeFragment.CreativeId.Should().Be(creativeId);
			result.CreativeFragment.Recipients.Should().HaveCount(1000);
			result.CreativeFragment.Service.BaseUrl.Should().Be(DefaultBaseUrl);
			result.CreativeFragment.Service.DealsEndpoint.Should().Be("deals");
			result.CreativeFragment.Service.UnsubscribeEndpoint.Should().Be("unsubscribe");
		}

		private void CreateFragment(string creativeId)
		{
			var taskId = ServiceActions.ExecuteTask(new CreateCreativeFragmentsTask
										   {
											   CreativeId = creativeId,
											   RecipientsPerFragment = 100
										   });

			WaitForTaskToComplete(taskId);
		}

		private string CreateCreative()
		{
			var listId = UIActions.CreateListWithRandomContacts("MyList", 100);
			var unsubscribeTenplateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x => x.Body = "body");
			var creativeId = UIActions.CreateSimpleCreative(new[] { listId }, unsubscribeTenplateId);
			return creativeId;
		}
	}
}
