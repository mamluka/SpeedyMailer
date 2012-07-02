using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	[TestFixture]
	public class UpdateDroneCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenADrone_ShouldUpdateTheStore()
		{
			var drone = new Drone
			            	{
									Hostname = "localhost:88888",
									Id = "drone/1",
									Status = DroneStatus.Online
			            	};

			ServiceActions.ExecuteCommand<UpdateDroneCommand>(x=> x.Drone = drone);

			var result = Load<Drone>(drone.Id);

			result.Hostname.Should().Be(drone.Hostname);
			result.Status.Should().Be(drone.Status);
		}
	}
}
