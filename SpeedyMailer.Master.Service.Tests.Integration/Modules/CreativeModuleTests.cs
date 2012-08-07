using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Protocol;
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
			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative();

			CallAddEndpoint(creativeId);

			WaitForEntitiesToExist<CreativeFragment>(1);
			var result = Query<CreativeFragment>().First();

			result.Recipients.Should().HaveCount(100);
			result.CreativeId.Should().Be(creativeId);
		}

		private void CallAddEndpoint(string creativeId)
		{
			var api = MasterResolve<Api>();
			api.Call<CreativeEndpoint.Add>(x => x.CreativeId = creativeId);
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
