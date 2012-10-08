using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Tests.Acceptance.DroneManagments
{
	[TestFixture]
	public class DroneBroadcastsHimSelfToTheService : IntegrationTestBase
	{
		[Test]
		public void DronesWillBroadcastThemSelfsToTheServiceWhenStarted()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			var drone1 = DroneActions.CreateDrone("drone1", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone1.Initialize();
			drone1.Start();

			var drone2 = DroneActions.CreateDrone("drone2", IntergrationHelpers.GenerateRandomLocalhostAddress(), DefaultBaseUrl);
			drone2.Initialize();
			drone2.Start();

			Store.WaitForEntitiesToExist<Drone>(2);

			var result = Store.Query<Drone>();

			result.Should().Contain(x => x.Id == "drone1");
			result.Should().Contain(x => x.Id == "drone2");
		}
	}
}
