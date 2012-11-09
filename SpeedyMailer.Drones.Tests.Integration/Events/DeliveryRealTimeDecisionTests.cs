using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Quartz;
using Quartz.Impl.Matchers;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class DeliveryRealTimeDecisionTests : IntegrationTestBase
	{
		public DeliveryRealTimeDecisionTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenGivenABounceEventAndTheBounceAnalyzerSayingItsABadBounce_ShouldNotStopSendingForTheGivenGroup()
		{
			DroneActions.EditSettings<EmailingSettings>(x =>
			{
				x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
				x.MailingDomain = "example.com";

			});

			DroneActions.StoreCollection(new[]
				                             {
					                             AddCreativePackage(),
					                             AddCreativePackage()
				                             });

			var task = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.FromName = "davud";
																		x.FromAddressDomainPrefix = "sales";
																	},
																x => x.WithIntervalInHours(1)
				);

			DroneActions.StartScheduledTask(task);

			FireEvent<DeliveryRealTimeDecision, AggregatedMailBounced>(x =>
																		   {
																			   x.MailEvents = new List<MailBounced>
						                                                                          {
							                                                                          new MailBounced
								                                                                          {
									                                                                          DomainGroup = "gmail",
																											  Recipient = "david@gmail.com",
																											  Message = "message meaning its a bad bounce"
								                                                                          }
						                                                                          };
																		   });

			var scheduler = DroneResolve<IScheduler>();

			var groups = scheduler.GetJobGroupNames();

			var jobKeys = groups.SelectMany(x => scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));

			jobKeys.Should().NotContain(x => x.Name.Contains("SendCreativePackagesWithIntervalTask"));
		}

		private static CreativePackage AddCreativePackage()
		{
			return new CreativePackage
					   {
						   Group = "gmail",
						   Subject = "test",
						   Body = "body",
						   To = "david@david.com"

					   };
		}
	}
}
