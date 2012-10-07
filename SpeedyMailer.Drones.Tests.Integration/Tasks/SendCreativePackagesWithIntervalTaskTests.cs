using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Runner;
using NUnit.Framework;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class SendCreativePackagesWithIntervalTaskTests:IntegrationTestBase
	{
		[Test]
		public void Execute_WhenTaskHasAnInterval_ShouldSendThePackagesAccordingToTheIntervalSpecified()
		{
			MongoRunner.Start();

			DroneActions.Store();

			var task = new SendCreativePackagesWithIntervalTask();

			MongoRunner.Shutdown();
		}
	}
}
