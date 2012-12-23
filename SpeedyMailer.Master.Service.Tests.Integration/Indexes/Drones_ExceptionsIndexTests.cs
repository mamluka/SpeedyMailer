using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
	public class Drones_ExceptionsIndexTests : IntegrationTestBase
	{
		[Test]
		public void Indexx_WhenCalled_ShouldMapReduceTheExceptionsForAllDrones()
		{
			var drones = new[]
				{
					new Drone
						{
							Domain = "drone1.com",
							Exceptions = new List<DroneException>
								{
									new DroneException {Exception = "exception1", Message = "message1", Component = "c",Time = "1/1/1 2:2:2"},
									new DroneException {Exception = "exception2", Message = "message2", Component = "c",Time = "1/1/1 2:2:2"},
								}
						},

					new Drone
						{
							Domain = "drone2.com",
							Exceptions = new List<DroneException>
								{
									new DroneException {Exception = "exception3", Message = "message3", Component = "c",Time = "1/1/1 2:2:2"},
									new DroneException {Exception = "exception4", Message = "message4", Component = "c",Time = "1/1/1 2:2:2"},
								}
						}
				}.ToList();

			drones.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Drones_Exceptions.ReduceResult, Drones_Exceptions>();
			var result = Store.Query<Drones_Exceptions.ReduceResult, Drones_Exceptions>(x => x.Group == "All");

			result[0].Exceptions.Should().HaveCount(4);

			result[0].Exceptions.Should().Contain(x => x.Contains("1/1/1 2:2:2 drone2.com c message3 exception3"));
			result[0].Exceptions.Should().Contain(x => x.Contains("1/1/1 2:2:2 drone2.com c message4 exception4"));
			result[0].Exceptions.Should().Contain(x => x.Contains("1/1/1 2:2:2 drone1.com c message1 exception1"));
			result[0].Exceptions.Should().Contain(x => x.Contains("1/1/1 2:2:2 drone1.com c message2 exception2"));
		}
	}
}
