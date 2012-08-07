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
			_scheduledTaskManager = DroneResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenCalled_ShouldContactTheMasterServiceAndRegisterTheDrone()
		{
			const string identifier = "drone1";

			DroneActions.EditSettings<DroneSettings>(x=>
			                                          	{
			                                          		x.Identifier = identifier;
			                                          	});

			DroneActions.EditSettings<ApiCallsSettings>(x =>
			                                            	{
			                                            		x.ApiBaseUri = DefaultBaseUrl;
			                                            	});
			
			

		    ListenToApiCall<ServiceEndpoints.RegisterDrone>();
			var task = new BroadcastDroneToServiceTask();
			_scheduledTaskManager.AddAndStart(task);

			AssertApiCalled<ServiceEndpoints.RegisterDrone>(x => x.Identifier == identifier);
		}
	}
}
