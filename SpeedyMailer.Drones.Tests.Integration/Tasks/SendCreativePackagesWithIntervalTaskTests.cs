using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using MongoDB.Runner;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class SendCreativePackagesWithIntervalTaskTests : IntegrationTestBase
	{
		public SendCreativePackagesWithIntervalTaskTests()
			: base(x => x.UseMongo = true)
		{

		}

		[Test]
		public void Execute_WhenTaskHasAnInterval_ShouldSendThePackagesAccordingToTheIntervalSpecified()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", 5), 
										   CreatePackage("david2@gmail.com", 5), 
										   CreatePackage("david3@gmail.com", 5)
				                       };

			DroneActions.StorCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x => x.Interval = 5, x => x.WithIntervalInSeconds(5).RepeatForever());

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

		}

		[Test]
		public void Execute_WhenPckagesArePresent_ShouldDeleteThemAfterSendingThem()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", 5), 
										   CreatePackage("david2@gmail.com", 5), 
										   CreatePackage("david3@gmail.com", 5)
				                       };

			DroneActions.StorCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x => x.Interval = 5, x => x.WithIntervalInSeconds(5).RepeatForever());

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

			var packages = DroneActions.FindAll<CreativePackage>();
			packages.Should().BeEmpty();
		}

		[Test]
		public void Execute_WhenTaskHasAnIntervalAndNoPackagesWereFound_ShouldStopExecutingTheTaskAndRemoveIt()
		{
			DroneActions.EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory);

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", 5), 
										   CreatePackage("david2@gmail.com", 5), 
										   CreatePackage("david3@gmail.com", 5)
				                       };

			DroneActions.StorCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x => x.Interval = 5, x => x.WithIntervalInSeconds(5).RepeatForever());

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

			Thread.Sleep(6000);

			Tasks.AssertTaskIsNotRunning(task);
		}

		private static CreativePackage CreatePackage(string email, int interval)
		{
			return new CreativePackage
					   {
						   Body = "body",
						   Interval = interval,
						   Subject = "subject",
						   To = email
					   };
		}
	}
}
