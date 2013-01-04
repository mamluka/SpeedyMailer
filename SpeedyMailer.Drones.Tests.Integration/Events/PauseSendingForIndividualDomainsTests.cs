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

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@blocked.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@suck.com",
							Domain = "suck.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a block",
							Recipient = "david@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a not block",
							Recipient = "david2@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
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

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@blocked.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@suck.com",
							Domain = "suck.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a block",
							Recipient = "david@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a not block",
							Recipient = "david2@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
				});

			AssertEventWasPublished<BlockingGroups>(x =>
															  x.Groups.Should().BeEquivalentTo(new[] { "blocked.com", "suck.com" }));
		}

		[Test]
		public void Inspect_WhenSomeMassesgesWereFoundToBeBlocking_ShouldSetThemAsProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule {Condition = "not a rule", Type = Classification.HardBounce},
							new HeuristicRule {Condition = "this is a block", Type = Classification.TempBlock, Data = new HeuristicData {TimeSpan = TimeSpan.FromHours(4)}},
						}
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
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@blocked.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@suck.com",
							Domain = "suck.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a block",
							Recipient = "david@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a not block",
							Recipient = "david2@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
				});

			var result = DroneActions.FindAll<CreativePackage>();

			result.Should().Contain(x => x.To == "david@blocked.com" && x.Processed);
			result.Should().Contain(x => x.To == "another-david@blocked.com" && x.Processed);
			result.Should().Contain(x => x.To == "another-david@suck.com" && x.Processed);
		}

		[Test]
		public void Inspect_GroupPoliciesAreAlreadyInTheStore_ShouldSetTheDomainsAreUndelierable()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			DroneActions.Store(new DeliverabilityClassificationRules
				{
					Rules = new List<HeuristicRule>
						{
							new HeuristicRule {Condition = "not a rule", Type = Classification.HardBounce},
							new HeuristicRule {Condition = "this is a block", Type = Classification.TempBlock, Data = new HeuristicData {TimeSpan = TimeSpan.FromHours(4)}},
						}
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
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@blocked.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@suck.com",
							Domain = "suck.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a block",
							Recipient = "david@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a not block",
							Recipient = "david2@gmail.com",
							Domain = "gmail.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
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

			var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$" ,},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$",},
                    new CreativePackage { To = "another-david@suck.com" ,Group = "$default$",}
                };

			DroneActions.StoreCollection(creativePackages);

			FireEvent<PauseSendingForIndividualDomains, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
				{
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "this is a block",
							Recipient = "david@blocked.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "$default$",
							Message = "not a block",
							Recipient = "david@suck.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a block",
							Recipient = "david@gmail.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.TempBlock, TimeSpan = TimeSpan.FromHours(2)}
						},
					new MailBounced
						{
							DomainGroup = "gmail",
							Message = "this is a not block",
							Recipient = "david2@gmail.com",
							Domain = "blocked.com",
							Classification = new MailClassfication {Type = Classification.NotClassified, TimeSpan = TimeSpan.FromHours(2)}
						}
				});

			var result = DroneActions.FindSingle<GroupsAndIndividualDomainsSendingPolicies>();

			result.GroupSendingPolicies.Should().ContainKeys(new[] { "blocked.com" });
			result.GroupSendingPolicies.Should().NotContainKey("suck.com");

			result.GroupSendingPolicies["blocked.com"].ResumeAt.Should().BeAtLeast(TimeSpan.FromHours(3));
		}
	}
}
