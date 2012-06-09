using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tasks;
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
			_scheduledTaskManager = DroneResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenCalled_ShouldContactTheMasterServiceAndRegisterTheDrone()
		{
			const string identifier = "drone1";

			DroneActions.EditSettings<IMasterServiceSettings>(x=>
			                                           	{
			                                           		x.Hostname = "localhost";
			                                           		x.Port = 2589;
			                                           	});

			DroneActions.EditSettings<IDroneSettings>(x=>
			                                          	{
			                                          		
			                                          		x.Identifier = identifier;
			                                          	});
			ServiceActions.Start();
			var task = new RegisterDroneWithMasterServiceTask();
			_scheduledTaskManager.Start(task);

			var result = Query<Shared.Domain.Drone>();

			result.Should().HaveCount(1);
			result.First().Identifier.Should().Be(identifier);
		}
	}

	public interface IDroneSettings
	{
		string Identifier { get; set; }
	}

	public interface IMasterServiceSettings
	{
		string Hostname { get; set; }
		int Port { get; set; }
	}
}
