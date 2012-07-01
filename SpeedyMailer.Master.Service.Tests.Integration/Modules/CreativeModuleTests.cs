using System.Linq;
using NUnit.Framework;
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
		public void Add_WhenGivenACreativeId_ShouldAddCreativeFragments()
		{
			ServiceActions.Initialize();
			ServiceActions.Start();

			var creativeId = CreateCreative();

			AddCreative(creativeId);

			WaitForEntityToExist<CreativeFragment>(1);
			var result = Query<CreativeFragment>().First();

			result.Recipients.Should().HaveCount(100);
			result.Creative.Id.Should().Be(creativeId);
		}

		private void AddCreative(string creativeId)
		{
			Api.Call<CreativeEndpoint.Add>()
				.WithParameters(x => x.CreativeId = creativeId)
				.Post();
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
