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
							MailBounced = new List<MailBounced>
								{
									new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com", CreativeId = "creative/1", Message = "this is bounce message1"},
									new MailBounced {DomainGroup = "msn", Recipient = "smith@msn.com", CreativeId = "creative/1", Message = "this is bounce message2"}
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
									new MailBounced {DomainGroup = "msn", Recipient = "david@msn.com", CreativeId = "creative/1", Message = "this is bounce message3"},
									new MailBounced {DomainGroup = "msn", Recipient = "ohh@msn.com", CreativeId = "creative/1", Message = "this is bounce message4"},
									new MailBounced {DomainGroup = "msn", Recipient = "yeah@msn.com", CreativeId = "creative/1", Message = "this is bounce message5"},
									new MailBounced {DomainGroup = "msn", Recipient = "cool@msn.com", CreativeId = "creative/2", Message = "this is bounce message6"}
								}
						}
				}.ToList();

			snapshots.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>();

			var result = Store.Query<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>(x => x.CreativeId == "creative/1");

			result.Should().Contain(x => x.CreativeId == "creative/1");
			result[0].Bounced.Select(x => x.Recipient).Should().BeEquivalentTo(new[] { "david@msn.com", "smith@msn.com", "david@msn.com", "yeah@msn.com", "ohh@msn.com" });
			result[0].Bounced.Select(x => x.Message).Should().BeEquivalentTo(new[] { "this is bounce message1", "this is bounce message2", "this is bounce message3", "this is bounce message4", "this is bounce message5" });
		}
	}
}