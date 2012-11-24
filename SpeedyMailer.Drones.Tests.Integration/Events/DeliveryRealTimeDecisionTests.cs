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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
								   {
									   HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
									   IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "bad bounce",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
								   });

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
			Jobs.Drone().AssertJobWasRemoved<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition =  "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
				IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "DNSBL",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
			});

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
			Jobs.Drone().AssertJobWasRemoved<SendCreativePackagesWithIntervalTask.Data>(x => x.Group == "gmail");
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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition =  "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
				IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "DNSBL",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition =  "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
				IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "DNSBL",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
			});

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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition =  "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
				IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "bad bounce",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
			});
			
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

			DroneActions.WaitForDocumentToExist<IpBlockingGroups>();

			var result = DroneActions.FindSingle<IpBlockingGroups>();

			result.Groups.Should().OnlyContain(x => x == "gmail");
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

			DroneActions.Store(new UnDeliveredMailClassificationHeuristicsRules
			{
				HardBounceRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition =   "account.+?disabled",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
							                                     
						                                     },
				IpBlockingRules = new List<HeuristicRule>
						                                     {
																 new HeuristicRule
																	 {
																		 Condition = "ip blocked",
																		 TimeSpan = TimeSpan.FromHours(2)
																	 }
						                                     }
			});

			FireEvent<DeliveryRealTimeDecision, AggregatedMailDeferred>(x =>
																		   {
																			   x.MailEvents = new List<MailDeferred>
						                                                                          {
							                                                                          new MailDeferred()
								                                                                          {
									                                                                          DomainGroup = "gmail",
									                                                                          Recipient = "david@gmail.com",
									                                                                          Message = "ip blocked"
								                                                                          }
						                                                                          };
																		   });

			DroneActions.WaitForDocumentToExist<IpBlockingGroups>();

			var result = DroneActions.FindSingle<IpBlockingGroups>();

			result.Groups.Should().OnlyContain(x => x == "gmail");
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
