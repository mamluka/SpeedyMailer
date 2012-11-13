using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Master.Service.Apis;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;
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
																					BaseUrl = DefaultBaseUrl
			                                                                 	});
			Store.Store(new CreativeFragment
			      	{
			      		Id = "creativefragment/1"
			      	});

			Api.ListenToApiCall<DroneEndpoints.Admin.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			Api.AssertApiCalled<DroneEndpoints.Admin.Wakeup>();
		}

		[Test]
		public void Execute_WhenThereAreNoFragments_ShouldNotWakeupAnyDrones()
		{
			Api.ListenToApiCall<DroneEndpoints.Admin.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			Api.AssertApiWasntCalled<DroneEndpoints.Admin.Wakeup>();
		}

		[Test]
		public void Execute_WhenThereAreNoDrones_ShouldDoNothing()
		{
			Store.Store(new CreativeFragment
			{
				Id = "creativefragment/1"
			});

			Api.ListenToApiCall<DroneEndpoints.Admin.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			Api.AssertApiWasntCalled<DroneEndpoints.Admin.Wakeup>();
		}

		[Test]
		public void Execute_WhenDroneIsNotAsleep_ShouldNotCallIt()
		{
			ServiceActions.ExecuteCommand<UpdateDroneCommand>(x => x.Drone = new Drone
			{
				Status = DroneStatus.Online,
				BaseUrl = DefaultBaseUrl
			});

			Store.Store(new CreativeFragment
			{
				Id = "creativefragment/1"
			});

			Api.ListenToApiCall<DroneEndpoints.Admin.Wakeup>(DefaultBaseUrl);

			var task = new WakeupSleepingDronesTask();
			ServiceActions.StartScheduledTask(task);

			Api.AssertApiWasntCalled<DroneEndpoints.Admin.Wakeup>();
		}

	}
}
