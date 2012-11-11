using MongoDB.Bson;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class StopSpecificSendingJobsCommandTests : IntegrationTestBase
	{
		public StopSpecificSendingJobsCommandTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenSendingTaskIsRunningWithGroups_ShouldStopOnlyTheGroupGiven()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
			{
				x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
				x.MailingDomain = "example.com";

			});

			DroneActions.StoreCollection(new[]
				                             {
					                             AddCreativePackage("gmail"),
					                             AddCreativePackage("gmail"),
					                             AddCreativePackage("hotmail"),
					                             AddCreativePackage("hotmail")
				                             });

			var task1 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.FromName = "david";
																		x.FromAddressDomainPrefix = "sales";
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInHours(1)
				);

			var task2 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.FromName = "david";
																		x.FromAddressDomainPrefix = "sales";
																		x.Group = "hotmail";
																	},
																x => x.WithIntervalInHours(1)
				);

			DroneActions.StartScheduledTask(task1);
			DroneActions.StartScheduledTask(task2);

			DroneActions.ExecuteCommand<StopSpecificSendingJobsCommand>(x => x.Group = "gmail");

			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "hotmail");
		}

		private static CreativePackage AddCreativePackage(string domainGroup)
		{
			return new CreativePackage
			{
				Group = domainGroup,
				Subject = "test",
				Body = "body",
				To = "david@david.com"

			};
		}
	}
}
