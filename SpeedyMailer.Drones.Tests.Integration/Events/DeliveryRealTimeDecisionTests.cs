using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
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
		public void Inspect_WhenGivenABounceMailEventAndTheAnalyzerSayingItsBlockingIpBounce_ShouldStopSendingForTheGivenGroup()
		{
			SetSettings();

			DroneActions.StoreCollection(new[]
				                             {
					                             AddCreativePackage("gmail"),
					                             AddCreativePackage("gmail"),
					                             AddCreativePackage("hotmail"),
					                             AddCreativePackage("hotmail")
				                             });

			var task1 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);

			var task2 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "hotmail";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);

			DroneActions.StartScheduledTask(task1);
			DroneActions.StartScheduledTask(task2);

			Jobs.Drone().WaitForJobToStart(task1);
			Jobs.Drone().WaitForJobToStart(task2);

			FireEvent<DeliveryRealTimeDecision, AggregatedMailBounced>(x =>
				{
					x.MailEvents = new List<MailBounced>
						{
							new MailBounced
								{
									DomainGroup = "gmail",
									Recipient = "david@gmail.com",
									Message = "message meaning its a bad bounce",
									Type = new MailClassfication {Classification = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
								}
						};
				});

			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "hotmail");
			Jobs.Drone().AssertJobWasPaused<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
		}

		[Test]
		public void Inspect_WhenGivenABounceMailEventAndTheAnalyzerSayingItsBlockingIpBounceAndTheGroupIsDefault_ShouldDoNothing()
		{
			SetSettings();

			DroneActions.StoreCollection(new[]
				                             {
					                             AddCreativePackage("$default$"),
				                             });

			var task1 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "$default$";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);


			DroneActions.StartScheduledTask(task1);
			Jobs.Drone().WaitForJobToStart(task1);

			StoreClassificationRules(new[]
				{
					new HeuristicRule {Condition = "account.+?disabled", Type = Classification.HardBounce},
					new HeuristicRule {Condition = "bad bounce", Type = Classification.TempBlock, Data = new HeuristicData{TimeSpan = TimeSpan.FromHours(2)}}
				});

			FireEvent<DeliveryRealTimeDecision, AggregatedMailBounced>(x =>
																		   {
																			   x.MailEvents = new List<MailBounced>
						                                                                          {
							                                                                          new MailBounced
								                                                                          {
									                                                                          DomainGroup = "$default$",
									                                                                          Recipient = "david@somedomain.com",
									                                                                          Message = "message meaning its a bad bounce"
								                                                                          }
						                                                                          };
																		   });

			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "$default$");
		}

		private void SetSettings()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = "dev/null";
																x.MailingDomain = "example.com";
															});
		}

		private void StoreClassificationRules(IEnumerable<HeuristicRule> heuristicRule)
		{
			DroneActions.Store(new DeliverabilityClassificationRules
								   {
									   Rules = heuristicRule.ToList()
								   });
		}

		[Test]
		public void Inspect_WhenRulesAreNotMatchingForBouncedMail_ShouldDoNothing()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
			{
				x.WritingEmailsToDiskPath = "dev/null";
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
																		x.Group = "gmail";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);

			var task2 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "hotmail";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);

			DroneActions.StartScheduledTask(task1);
			DroneActions.StartScheduledTask(task2);

			Jobs.Drone().WaitForJobToStart(task1);
			Jobs.Drone().WaitForJobToStart(task2);

			StoreClassificationRules(new[]
				{
					new HeuristicRule {Condition = "account.+?disabled", Type = Classification.HardBounce},
					new HeuristicRule {Condition = "DNSBL", Type = Classification.TempBlock, Data = new HeuristicData {TimeSpan = TimeSpan.FromHours(2)}}
				});

			FireEvent<DeliveryRealTimeDecision, AggregatedMailBounced>(x =>
																		   {
																			   x.MailEvents = new List<MailBounced>
						                                                                          {
							                                                                          new MailBounced
								                                                                          {
									                                                                          DomainGroup = "gmail",
									                                                                          Recipient = "david@gmail.com",
									                                                                          Message = "message meaning its a good bounce"
								                                                                          }
						                                                                          };
																		   });


			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "hotmail");
			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
		}

		[Test]
		public void Inspect_WhenGivenABounceMailEventAndTheAnalyzerSayingItsBlockingIpBounce_ShouldPersistTheNonSendingPolicy()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.EditSettings<EmailingSettings>(x =>
															{
																x.WritingEmailsToDiskPath = "dev/null";
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
																		 x.Group = "gmail";
																	 },
																 x => x.WithIntervalInHours(1).RepeatForever()
				);

			var task2 = new SendCreativePackagesWithIntervalTask(x =>
																	 {
																		 x.Group = "hotmail";
																	 },
																 x => x.WithIntervalInHours(1).RepeatForever()
				);

			DroneActions.StartScheduledTask(task1);
			DroneActions.StartScheduledTask(task2);

			Jobs.Drone().WaitForJobToStart(task1);
			Jobs.Drone().WaitForJobToStart(task2);

			FireEvent<DeliveryRealTimeDecision, AggregatedMailBounced>(x =>
				{
					x.MailEvents = new List<MailBounced>
						{
							new MailBounced
								{
									DomainGroup = "gmail",
									Recipient = "david@gmail.com",
									Message = "message meaning its a bad bounce",
									Type = new MailClassfication {Classification = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
								}
						};
				});

			DroneActions.WaitForDocumentToExist<GroupsAndIndividualDomainsSendingPolicies>();

			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKey("gmail");
		}

		private static CreativePackage AddCreativePackage(string domainGroup)
		{
			return new CreativePackage
					   {
						   Group = domainGroup,
						   Subject = "test",
						   HtmlBody = "body",
						   To = "david@david.com",
						   FromAddressDomainPrefix = "david",
						   Interval = 10,
						   FromName = "sales"

					   };
		}
	}
}
