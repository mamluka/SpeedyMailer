using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class PauseSendingForIndividualDomainsTests : IntegrationTestBase
	{
		public PauseSendingForIndividualDomainsTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Inspect_WhenGivenABouncedMessageClassifiedAsBlockingIpAndTheGroupIsDefault_ShouldSetTheDomainsAreUndelierable()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "not a rule" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@suck.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKeys(new[] { "blocked.com", "suck.com" });
			result.GroupSendingPolicies["blocked.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
			result.GroupSendingPolicies["suck.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
		}

		[Test]
		public void Inspect_WhenPausingAnEvent_ShouldFireAnEvent()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<BlockingGroups>();

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "not a rule" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});


			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@suck.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });

			AssertEventWasPublished<BlockingGroups>(x => x.Groups.Any(s => s == "suck.com"));
			AssertEventWasPublished<BlockingGroups>(x => x.Groups.Any(s => s == "blocked.com"));
		}

		[Test]
		public void Inspect_WhenSomeMassesgesWereFoundToBeBlocking_ShouldSetThemAsProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
								   {
									   HardBounceRules = new List<string> { "not a rule" },
									   BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
								   });

			var creativePackages = new[]
				                       {
					                       new CreativePackage {To = "david@blocked.com", Group = "$default$", },
					                       new CreativePackage {To = "another-david@blocked.com", Group = "$default$", },
					                       new CreativePackage {To = "another-david@suck.com", Group = "gmail", }
				                       };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				                                                                                       {
					                                                                                       new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
					                                                                                       new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@suck.com"},
					                                                                                       new MailBounced {DomainGroup = "gmail", Message = "this is a block", Recipient = "david@gmail.com"},
					                                                                                       new MailBounced {DomainGroup = "gmail", Message = "this is a not block", Recipient = "david2@gmail.com"}
				                                                                                       });

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().Contain(x => x.To == "david@blocked.com" && x.Processed);
			result.Should().Contain(x => x.To == "another-david@blocked.com" && x.Processed);
			result.Should().Contain(x => x.To == "another-david@suck.com" && x.Processed);
		}

		[Test]
		public void Inspect_GroupPliciesAreAlreadyInTheStore_ShouldSetTheDomainsAreUndelierable()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "not a rule" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});

			DroneActions.Store(new GroupsAndIndividualDomainsSendingPolicies
								   {
									   GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>()
								   });

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@suck.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKeys(new[] { "blocked.com", "suck.com" });
			result.GroupSendingPolicies["blocked.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
			result.GroupSendingPolicies["suck.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
		}

		[Test]
		public void Inspect_WhenOnlyPartOfThePackagesInDefaultGroupNeedsToBePaused_ShouldOnlySetThatDomainAsUndeliverable()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "not a rule" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "$default$", Message = "not a block", Recipient = "david@suck.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKeys(new[] { "blocked.com" });
			result.GroupSendingPolicies.Should().NotContainKey("suck.com");

			result.GroupSendingPolicies["blocked.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
		}

		[Test]
		public void Inspect_WhenBlockedMailedAreFoundButNoneExistInTheDatabase_ShouldDoNothing()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "hard bounce" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$"},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$"}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "hard bounce", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });

			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.Should().BeNull();
		}

		[Test]
		public void Inspect_WhenGivenADeferredMessageClassifiedAsBlockingIpAndTheGroupIsDefault_ShouldSetAllEmailsThatHaveThisDomainAsUndeliverable()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
			{
				HardBounceRules = new List<string> { "not a rule" },
				BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
			});

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailDeferred>(x => x.MailEvents = new List<MailDeferred>
                {
                    new MailDeferred {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailDeferred {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailDeferred {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKeys(new[] { "blocked.com" });
			result.GroupSendingPolicies["blocked.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
		}
	}
}
