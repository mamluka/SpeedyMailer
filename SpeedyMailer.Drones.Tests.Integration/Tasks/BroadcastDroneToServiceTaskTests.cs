using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class BroadcastDroneToServiceTaskTests : IntegrationTestBase
	{
		private IScheduledTaskManager _scheduledTaskManager;

		public override void ExtraSetup()
		{
			_scheduledTaskManager = MasterResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenCalled_ShouldContactTheMasterServiceAndRegisterTheDrone()
		{
			const string identifier = "drone1";

			DroneActions.EditSettings<IDroneSettings>(x=>
			                                          	{
			                                          		x.Identifier = identifier;
			                                          	});

		    ListenToApiCall<ServiceEndpoints.RegisterDrone>();
			var task = new BroadcastDroneToServiceTask(x=> x.Identifier = identifier);
			_scheduledTaskManager.AddAndStart(task);

			AssertApiCalled<ServiceEndpoints.RegisterDrone>(x => x.Identifier == identifier);
		}
	}
}
