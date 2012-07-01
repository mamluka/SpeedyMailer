using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Tasks
{
	[TestFixture]
	public class WakeupSleepingDronesTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenThereAreNoFragments_ShouldNotWakeupAnyDrones()
		{
			var drone1 = DroneActions.CreateDrone("drone/1");

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);
		}
	}
}
