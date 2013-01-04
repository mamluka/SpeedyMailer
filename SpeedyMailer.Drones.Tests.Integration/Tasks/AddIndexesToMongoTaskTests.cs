using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class AddIndexesToMongoTaskTests : IntegrationTestBase
	{
		public AddIndexesToMongoTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenExecuted_ShouldWriteTheCreativePackagesIndexes()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			var task = new AddIndexesToMongoTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForIndexesToExist();

			var result = DroneActions.GetIndexes();

			result.Should().BeEquivalentTo(new[] { "_id_", "CreativePackage_Group_Processed", "CreativePackage_Processed", "CreativePackage_To" });
		}
	}
}
