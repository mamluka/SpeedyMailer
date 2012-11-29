using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class SendDroneStateSnapshotTaskTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenExecuted_ShouldSendTheBasicDroneDetails()
		{
			DroneActions.EditSettings<DroneSettings>();

			Api.ListenToApiCall<ServiceEndpoints.Drones.SendStateSnapshot>();

			DroneActions.StartScheduledTask(new SendDroneStateSnapshotTask());

			Api.AssertApiCalled<ServiceEndpoints.Drones.SendStateSnapshot>(x=>
				                                                               {
					                                                               x.Drone.
				                                                               });
		}
	}
}
