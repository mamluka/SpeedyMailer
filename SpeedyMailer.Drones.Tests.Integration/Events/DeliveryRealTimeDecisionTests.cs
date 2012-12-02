using System;
using System.Collections.Generic;
using System.Threading;
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "bad bounce", TimeSpan = TimeSpan.FromHours(2) });

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
																		x.Group = "$default";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);


			DroneActions.StartScheduledTask(task1);
			Jobs.Drone().WaitForJobToStart(task1);

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "bad bounce", TimeSpan = TimeSpan.FromHours(2) });

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
		[Test]
		public void Inspect_WhenGivenADeferredMailEventAndTheAnalyzerSayingItsBlockingIpDeferAndTheGroupIsDefault_ShouldDoNothing()
		{
			SetSettings();

			DroneActions.StoreCollection(new[]
				                             {
					                             AddCreativePackage("$default$"),
				                             });

			var task1 = new SendCreativePackagesWithIntervalTask(x =>
																	{
																		x.Group = "$default";
																	},
																x => x.WithIntervalInHours(1).RepeatForever()
				);


			DroneActions.StartScheduledTask(task1);
			Jobs.Drone().WaitForJobToStart(task1);

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "bad bounce", TimeSpan = TimeSpan.FromHours(2) });

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																		   {
																			   x.MailEvents = new List<MailDeferred>
						                                                                          {
							                                                                          new MailDeferred()
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

		private void StoreClassificationRules(string bounceCondition, HeuristicRule heuristicRule)
		{
			DroneActions.Store(new DeliverabilityClassificationRules
								   {
									   HardBounceRules = new List<string>
						                                     {
							                                     bounceCondition,
						                                     },
									   BlockingRules = new List<HeuristicRule>
						                                   {
							                                   heuristicRule
						                                   }
								   });
		}

		[Test]
		public void Inspect_WhenGivenADeferredMailEventAndTheAnalyzerSayingItsBlockingIpDeferral_ShouldStopSendingForTheGivenGroup()
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "DNSBL", TimeSpan = TimeSpan.FromHours(2) });

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																		   {
																			   x.MailEvents = new List<MailDeferred>
						                                                                          {
							                                                                          new MailDeferred
								                                                                          {
									                                                                          DomainGroup = "gmail",
									                                                                          Recipient = "david@gmail.com",
									                                                                          Message = "deferred mail because of DNSBL"
								                                                                          }
						                                                                          };
																		   });

			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "hotmail");
			Jobs.Drone().AssertJobWasPaused<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "DNSBL", TimeSpan = TimeSpan.FromHours(2) });

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
		public void Inspect_WhenRulesAreNotMatchingForDeferredMail_ShouldDoNothing()
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "DNSBL", TimeSpan = TimeSpan.FromHours(2) });

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																			{
																				x.MailEvents = new List<MailDeferred>
						                                                                           {
							                                                                           new MailDeferred
								                                                                           {
									                                                                           DomainGroup = "gmail",
									                                                                           Recipient = "david@gmail.com",
									                                                                           Message = "deferred message because of real cause"
								                                                                           }
						                                                                           };
																			});



			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "hotmail");
			Jobs.Drone().AssertJobIsCurrentlyRunnnig<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
		}

		[Test]
		public void Inspect_WhenThereAreNoRules_ShouldDoNothing()
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

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																			{
																				x.MailEvents = new List<MailDeferred>
						                                                                           {
							                                                                           new MailDeferred
								                                                                           {
									                                                                           DomainGroup = "gmail",
									                                                                           Recipient = "david@gmail.com",
									                                                                           Message = "deferred message because of real cause"
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "bad bounce", TimeSpan = TimeSpan.FromHours(2) });

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

			DroneActions.WaitForDocumentToExist<GroupsSendingPolicies>();

			var result = DroneActions.FindSingle<GroupsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKey("gmail");
		}

		[Test]
		public void Inspect_WhenGivenADeferredMailEventAndTheAnalyzerSayingItsBlockingIpBounce_ShouldPersistTheNonSendingPolicy()
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

			StoreClassificationRules("account.+?disabled", new HeuristicRule { Condition = "ip blocked", TimeSpan = TimeSpan.FromHours(2) });

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																		   {
																			   x.MailEvents = new List<MailDeferred>
						                                                                          {
							                                                                          new MailDeferred
								                                                                          {
									                                                                          DomainGroup = "gmail",
									                                                                          Recipient = "david@gmail.com",
									                                                                          Message = "ip blocked"
								                                                                          }
						                                                                          };
																		   });

			DroneActions.WaitForDocumentToExist<GroupsSendingPolicies>();

			var result = DroneActions.FindSingle<GroupsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKey("gmail");
		}

		private static CreativePackage AddCreativePackage(string domainGroup)
		{
			return new CreativePackage
					   {
						   Group = domainGroup,
						   Subject = "test",
						   Body = "body",
						   To = "david@david.com",
						   FromAddressDomainPrefix = "david",
						   Interval = 10,
						   FromName = "sales"

					   };
		}
	}
}
