using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drone.Settings;
using SpeedyMailer.Drone.Tasks;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Shared;

namespace SpeedyMailer.Drone.Tests.Integration.Tasks
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
			_scheduledTaskManager.Start(task);

			AssertApiCall<ServiceApi.RegisterDrone.Request>(x => x.Identifier == identifier);
		}
	}
}
