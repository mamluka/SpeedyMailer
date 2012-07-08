using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Master.Service.Apis;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Master.Service.Tests.Integration.Commands;
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
		public void Execute_WhenCalled_ShouldWakeUpSleepingDrones()
		{
			ServiceActions.ExecuteCommand<UpdateDroneCommand>(x => x.Drone = new Drone
			                                                                 	{
																					Status = DroneStatus.Asleep,
																					Hostname = DefaultBaseUrl
			                                                                 	});
			Store(new CreativeFragment
			      	{
			      		Id = "creativefragment/1"
			      	});

			ListenToApiCall<DroneEndpoints.Manage.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			AssertApiCalled();
		}

		[Test]
		public void Execute_WhenThereAreNoFragments_ShouldNotWakeupAnyDrones()
		{
			ListenToApiCall<DroneEndpoints.Manage.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			AssertApiWasntCalled();
		}

		[Test]
		public void Execute_WhenThereAreNoDrones_ShouldDoNothing()
		{
			Store(new CreativeFragment
			{
				Id = "creativefragment/1"
			});

			ListenToApiCall<DroneEndpoints.Manage.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			AssertApiWasntCalled();
		}

		[Test]
		public void Execute_WhenDroneIsNotAsleep_ShouldNotCallIt()
		{
			ServiceActions.ExecuteCommand<UpdateDroneCommand>(x => x.Drone = new Drone
			{
				Status = DroneStatus.Online,
				Hostname = DefaultBaseUrl
			});
			Store(new CreativeFragment
			{
				Id = "creativefragment/1"
			});

			ListenToApiCall<DroneEndpoints.Manage.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			AssertApiWasntCalled();
		}

	}
}
