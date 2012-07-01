using NUnit.Framework;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	[TestFixture]
	public class RegisterDroneWithMasterServiceTaskTests : IntegrationTestBase
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

		    ListenToApiCall<ServiceApi.RegisterDrone,ServiceApi.RegisterDrone.Request>();
			var task = new RegisterDroneWithServiceTask(x=> x.Identifier = identifier);
			_scheduledTaskManager.AddAndStart(task);

			AssertApiCall<ServiceApi.RegisterDrone.Request>(x => x.Identifier == identifier);
		}
	}
}
