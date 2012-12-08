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
    class Creative_SendingReportIndexTests : IntegrationTestBase
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

            Store.WaitForIndexNotToBeStale<Creative_SendingReport.ReduceResult, Creative_SendingReport>();

            var result = Store.Query<Creative_SendingReport.ReduceResult, Creative_SendingReport>(x => x.CreativeId == "creative/1");

            result.Should().Contain(x => x.CreativeId == "creative/1");
            result.Should().Contain(x => x.TotalBounces == 6);
            result.Should().Contain(x => x.TotalDefers == 5);
            result.Should().Contain(x => x.TotalSends == 3);
        }
    }
}
