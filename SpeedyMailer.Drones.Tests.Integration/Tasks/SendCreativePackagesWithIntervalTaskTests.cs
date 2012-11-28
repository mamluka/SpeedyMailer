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
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class SendCreativePackagesWithIntervalTaskTests : IntegrationTestBase
	{
		public SendCreativePackagesWithIntervalTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenTaskHasAnInterval_ShouldSendThePackagesAccordingToTheIntervalSpecified()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", "gmail"), 
										   CreatePackage("david2@gmail.com", "gmail"), 
										   CreatePackage("david3@gmail.com", "gmail")
				                       };

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

		}

		[Test]
		public void Execute_WhenPckagesArePresent_ShouldDeleteThemAfterSendingThem()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", "gmail"), 
										   CreatePackage("david2@gmail.com", "gmail"), 
										   CreatePackage("david3@gmail.com", "gmail")
				                       };

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

			var packages = DroneActions.FindAll<CreativePackage>();
			packages.Should().OnlyContain(x => x.Processed);
		}

		[Test]
		public void Execute_WhenTaskHasAnIntervalAndNoPackagesWereFound_ShouldStopExecutingTheTaskAndRemoveIt()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackages = new[]
				                       {
					                       CreatePackage("david@gmail.com", "gmail"), 
										   CreatePackage("david2@gmail.com", "gmail"), 
										   CreatePackage("david3@gmail.com", "gmail")
				                       };

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients, 5);

			Thread.Sleep(6000);

			Tasks.AssertTaskIsNotRunning(task);
		}

		private static CreativePackage CreatePackage(string email, string group)
		{
			return new CreativePackage
					   {
						   Body = "body",
						   Group = group,
						   Subject = "subject",
						   To = email,
						   FromAddressDomainPrefix = "david",
						   Interval = 10,
						   FromName = "sales"
					   };
		}
	}
}
