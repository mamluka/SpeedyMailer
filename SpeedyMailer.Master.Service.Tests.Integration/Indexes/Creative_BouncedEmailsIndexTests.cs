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
    public class Creative_BouncedEmailsIndexTests : IntegrationTestBase
    {
        [Test]
        public void Index_WhenGivenSnapShots_ShouldMapReduceAllBounces()
        {
            var snapshots = new[]
                {
                     new DroneStateSnapshoot
                        {
                            MailSent = new List<MailSent>
                                {
                                    new MailSent {DomainGroup = "gmail", Recipient = "david@gmail.com", CreativeId = "creative/1"},
                                    new MailSent {DomainGroup = "hotmail", Recipient = "david@hotmail.com", CreativeId = "creative/1"}
                                },
                            MailDeferred = new List<MailDeferred>
                                {
                                    new MailDeferred {DomainGroup = "aol", Recipient = "david@aol.com", CreativeId = "creative/1"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "smith@aol.com", CreativeId = "creative/1"}
                                },
                            MailBounced = new List<MailBounced>
                                {
                                    new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com", CreativeId = "creative/1"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "smith@msn.com", CreativeId = "creative/1"}
                                }
                        },

                    new DroneStateSnapshoot
                        {
                            MailSent = new List<MailSent>
                                {
                                    new MailSent {DomainGroup = "gmail", Recipient = "moshe@gmail.com", CreativeId = "creative/2"},
                                },
                            MailDeferred = new List<MailDeferred>
                                {
                                    new MailDeferred {DomainGroup = "aol", Recipient = "shit@aol.com", CreativeId = "creative/1"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "mother@aol.com", CreativeId = "creative/1"},
                                    new MailDeferred {DomainGroup = "aol", Recipient = "fucker@aol.com", CreativeId = "creative/2"}
                                },
                            MailBounced = new List<MailBounced>
                                {
                                    new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com", CreativeId = "creative/1"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "ohh@msn.com", CreativeId = "creative/1"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "yeah@msn.com", CreativeId = "creative/1"},
                                    new MailBounced {DomainGroup = "msn", Recipient = "cool@msn.com", CreativeId = "creative/2"}
                                }
                        }
                }.ToList();

            snapshots.ForEach(Store.Store);

            Store.WaitForIndexNotToBeStale<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>();

            var result = Store.Query<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>(x => x.CreativeId == "creative/1");

            result.Should().Contain(x => x.CreativeId == "creative/1");
            result[0].Bounced.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@msn.com", "smith@msn.com", "david@msn.com", "yeah@msn.com", "ohh@msn.com" });
        }
    }
}
