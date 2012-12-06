using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
    public class MarkDefaultGroupdPackagesAsUndeliverableTests : IntegrationTestBase
    {
        public MarkDefaultGroupdPackagesAsUndeliverableTests()
            : base(x => x.UseMongo = true)
        { }

        [Test]
        public void Inspect_WhenGivenABouncedMessageClassifiedAsBlockingIpAndTheGroupIsDefault_ShouldSetAllEmailsThatHaveThisDomainAsUndeliverable()
        {
            DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

            DroneActions.Store(new DeliverabilityClassificationRules
            {
                HardBounceRules = new List<string> { "not a rule" },
                BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
            });

            var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$"},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$"}
                };

            DroneActions.StoreCollection(creativePackages);

            FireEvent<MarkDefaultGroupdPackagesAsUndeliverable, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



            var result = DroneActions.FindAll<CreativePackage>();

            result.Should().Contain(x => x.Id == creativePackages[0].Id && x.Processed);
            result.Should().Contain(x => x.Id == creativePackages[1].Id && x.Processed);
        }

        [Test]
        public void Inspect_WhenGivenABouncedMessageClassifiedAsBlockingIpAndTheGroupIsDefault_ShouldSetOnlyNonProcessedEmailsTAsUndeliverable()
        {
            DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

            DroneActions.Store(new DeliverabilityClassificationRules
            {
                HardBounceRules = new List<string> { "not a rule" },
                BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
            });

            var creativePackages = new[]
                {
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$"},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$"},
                    new CreativePackage { To = "already-sent@blocked.com" ,Group = "$default$",Processed = true}
                };

            DroneActions.StoreCollection(creativePackages);

            FireEvent<MarkDefaultGroupdPackagesAsUndeliverable, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



            var result = DroneActions.FindAll<CreativePackage>();

            result.Should().Contain(x => x.Id == creativePackages[2].Id && x.Processed);
        }

        [Test]
        public void Inspect_WhenGivenABouncedMessageClassifiedAsBouncedIpAndTheGroupIsDefault_ShouldDoNothing()
        {
            Assert.DoesNotThrow(() =>
                {
                    DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

                    DroneActions.Store(new DeliverabilityClassificationRules
                        {
                            HardBounceRules = new List<string> { "not a rule" },
                            BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
                        });

                    FireEvent<MarkDefaultGroupdPackagesAsUndeliverable, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                        {
                            new MailBounced {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                            new MailBounced {DomainGroup = "gmail", Message = "this is a block", Recipient = "david@gmail.com"},
                            new MailBounced {DomainGroup = "gmail", Message = "this is a not block", Recipient = "david2@gmail.com"}
                        });
                });
        }

        [Test]
        public void Inspect_WhenThereAreNoEvents_ShouldDoNothing()
        {
            Assert.DoesNotThrow(() =>
                {
                    DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

                    var creativePackages = new[]
                        {
                            new CreativePackage {To = "david@blocked.com", Group = "$default$"},
                            new CreativePackage {To = "another-david@blocked.com", Group = "$default$"}
                        };

                    DroneActions.StoreCollection(creativePackages);

                    DroneActions.Store(new DeliverabilityClassificationRules
                        {
                            HardBounceRules = new List<string> { "not a rule" },
                            BlockingRules = new List<HeuristicRule> { new HeuristicRule { Condition = "this is a block", TimeSpan = TimeSpan.FromHours(4) } }
                        });
                });
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

            FireEvent<MarkDefaultGroupdPackagesAsUndeliverable, AggregatedMailBounced>(x => x.MailEvents = new List<MailBounced>
                {
                    new MailBounced {DomainGroup = "$default$", Message = "hard bounce", Recipient = "david@blocked.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailBounced {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });

            var result = DroneActions.FindAll<CreativePackage>();

            result.Should().Contain(x => x.Id == creativePackages[0].Id && x.Processed);
            result.Should().Contain(x => x.Id == creativePackages[1].Id && x.Processed);
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
                    new CreativePackage { To = "david@blocked.com" ,Group = "$default$"},
                    new CreativePackage { To = "another-david@blocked.com" ,Group = "$default$"}
                };

            DroneActions.StoreCollection(creativePackages);

            FireEvent<MarkDefaultGroupdPackagesAsUndeliverable, AggregatedMailDeferred>(x => x.MailEvents = new List<MailDeferred>
                {
                    new MailDeferred {DomainGroup = "$default$", Message = "this is a block", Recipient = "david@blocked.com"},
                    new MailDeferred {DomainGroup = "gmail", Message = "this is a block",Recipient = "david@gmail.com"},
                    new MailDeferred {DomainGroup = "gmail", Message = "this is a not block",Recipient = "david2@gmail.com"}
                });



            var result = DroneActions.FindAll<CreativePackage>();

            result.Should().Contain(x => x.Id == creativePackages[0].Id && x.Processed);
            result.Should().Contain(x => x.Id == creativePackages[1].Id && x.Processed);
        }
    }
}
