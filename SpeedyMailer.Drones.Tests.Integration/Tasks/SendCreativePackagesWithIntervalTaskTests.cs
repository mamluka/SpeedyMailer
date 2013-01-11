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
			Email.AssertEmailsSentWithInterval(recipients, 5, 30);

		}

		[Test]
		public void Execute_WhenSending_ShouldIncreaseTheRetryCountByOne()
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
				                       };

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.RetryCount == 1);
		}

		[Test]
		public void Execute_WhenPackageWasAlreadySentThreeTimes_ShouldSetThePackageAndProcessedAndNotSendIt()
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
				                       };

			creativePackages[0].RetryCount = 3;

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForChangeOnStoredObject<CreativePackage>(x => x.Processed);
			Email.AssertEmailNotSentTo(new[] { "david@gmail.com" });
		}

		[Test]
		public void Execute_WhenThereIsATaskThatWasRecentlyTouched_ShouldSendItLast()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});
			var creativePackages = new[]
				{
					CreatePackage("david@gmail.com", "gmail", DateTime.UtcNow.AddMinutes(-10)),
					CreatePackage("david3@gmail.com", "gmail", DateTime.UtcNow.AddMinutes(-30)),
					CreatePackage("david2@gmail.com", "gmail", DateTime.UtcNow.AddMinutes(-20)),
				};


			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = new[] { "david3@gmail.com", "david2@gmail.com", "david@gmail.com" };
			Email.AssertEmailsSentInOrder(recipients, 30);

		}

		[Test]
		public void Execute_WhenTheGroupIsDefaultAndSomePackagesAreNotDeliverable_ShouldNotSendThatPackages()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
																x.MailingDomain = "example.com";

															});

			var creativePackages = new[]
				                       {
										   CreatePackage("david2@money.com", "$default$"), 
										   CreatePackage("david3@verygood.com", "$default$")
				                       };

			creativePackages[1].Processed = true;

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "$default$";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();

			Email.AssertEmailsSentTo(new[] { recipients[0] });
			Email.AssertEmailNotSentTo(new[] { recipients[1] });

		}

		[Test]
		public void Execute_WhenThereAreProcessedAndNonProcessed_ShouldSendOnlyTheUnprocessedPackages()
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

			creativePackages[2].Processed = true;

			DroneActions.StoreCollection(creativePackages);

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInSeconds(5).RepeatForever()
				);

			DroneActions.StartScheduledTask(task);

			var recipients = creativePackages.Select(x => x.To).ToList();
			Email.AssertEmailsSentWithInterval(recipients.Take(2).ToList(), 5, 30);
			Email.AssertEmailNotSentTo(new[] { recipients[2] }, 30);
		}

		[Test]
		public void Execute_WhenPckagesArePresent_ShouldSetThemAsProcessed()
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
			Email.AssertEmailsSentWithInterval(recipients, 5, 30);

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
			Email.AssertEmailsSentWithInterval(recipients, 5, 30);

			Thread.Sleep(6000);

			Tasks.AssertTaskIsNotRunning(task);
		}

		private static CreativePackage CreatePackage(string email, string group, DateTime touchDate = default(DateTime))
		{
			return new CreativePackage
					   {
						   HtmlBody = "body",
						   Group = group,
						   Subject = "subject",
						   To = email,
						   FromAddressDomainPrefix = "david",
						   Interval = 10,
						   FromName = "sales",
						   CreativeId = "creative/1",
						   TouchTime = touchDate

					   };
		}
	}
}
