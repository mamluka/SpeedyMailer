﻿using System;
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
    public class Creative_SentEmailsIndexTests : IntegrationTestBase
    {
        [Test]
        public void Index_WhenGivenSnapShots_ShouldMapReduceAllSentEmails()
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

            Store.WaitForIndexNotToBeStale<Creative_SentEmails.ReduceResult, Creative_SentEmails>();

            var result = Store.Query<Creative_SentEmails.ReduceResult, Creative_SentEmails>(x => x.CreativeId == "creative/1");

            result.Should().Contain(x => x.CreativeId == "creative/1");
            result[0].Sends.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@gmail.com", "david@hotmail.com"});
        }
    }
}
