using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
    public class CreativeExtendedSendingReportIndexTests : IntegrationTestBase
    {
        [Test]
        public void Index_WhenGivenSnapShots_ShouldMapReduceAllRawLogs()
        {
            var snapshots = new[]
                {
                    new DroneStateSnapshoot
                        {
                            CurrentCreativeId = "creative/1",
                            MailSent = new List<MailSent>
                                {
                                    new MailSent {DomainGroup = "gmail", Recipient = "david@gmail.com"},
                                    new MailSent {DomainGroup = "hotmail", Recipient = "david@hotmail.com"}
                                },
                            MailDeferred = new List<MailDeferred>
                                {
                                    new MailDeferred {DomainGroup = "aol", Recipient = "david@aol.com"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "smith@aol.com"}
                                },
                            MailBounced = new List<MailBounced>
                                {
                                    new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "smith@msn.com"}
                                }
                        },
                        
                        new DroneStateSnapshoot
                        {
                            CurrentCreativeId = "creative/1",
                            MailSent = new List<MailSent>
                                {
                                    new MailSent {DomainGroup = "gmail", Recipient = "moshe@gmail.com"},
                                },
                            MailDeferred = new List<MailDeferred>
                                {
                                    new MailDeferred {DomainGroup = "aol", Recipient = "shit@aol.com"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "mother@aol.com"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "fucker@aol.com"}
                                },
                            MailBounced = new List<MailBounced>
                                {
                                    new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "ohh@msn.com"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "yeah@msn.com"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "cool@msn.com"}
                                }
                        }
                }.ToList();

            snapshots.ForEach(Store.Store);

            Store.WaitForIndexNotToBeStale<Creative_ExtendedSendingReport.ReduceResult, Creative_ExtendedSendingReport>();

            var result = Store.Query<Creative_ExtendedSendingReport.ReduceResult, Creative_ExtendedSendingReport>(x => x.CreativeId == "creative/1");

            result.Should().Contain(x => x.CreativeId == "creative/1");
            result[0].Sends.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@gmail.com", "david@hotmail.com", "moshe@gmail.com" });
            result[0].Bounces.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@msn.com", "smith@msn.com", "david@msn.com", "ohh@msn.com", "yeah@msn.com", "cool@msn.com" });
            result[0].Defers.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@aol.com", "smith@aol.com", "shit@aol.com", "mother@aol.com", "fucker@aol.com" });
        }
    }
}
