using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Master.Service.Tests.Integration.Commands;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
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

		[Test]
		public void Add_WhenThereIsOneDroneAsleep_ShouldWakeUpSleepyDrone()
		{
			ServiceActions.Initialize();
			ServiceActions.Start();
			var drone = new Drone
			            	{
								Identifier = "drone1",
								Hostname="http://localhost:2587"
			            	};

			RegisterDroneWithService(drone);
			var creativeId = CreateCreative();
			AddCreative(creativeId);


		}

		private void RegisterDroneWithService(Drone drone)
		{
			//ServiceActions.ExecuteCommand<>()
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
