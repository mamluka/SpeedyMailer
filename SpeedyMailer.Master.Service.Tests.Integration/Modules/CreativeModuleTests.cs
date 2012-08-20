using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Web.Core.Commands;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
	[TestFixture]
	public class CreativeModuleTests :IntegrationTestBase
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

		private string CreateCreative()
		{
			var listId = UIActions.CreateListWithRandomContacts("MyList", 100);
			var unsubscribeTenplateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x => x.Body = "body");
			var creativeId = UIActions.CreateSimpleCreative(new[] {listId}, unsubscribeTenplateId);
			return creativeId;
		}
	}
}
