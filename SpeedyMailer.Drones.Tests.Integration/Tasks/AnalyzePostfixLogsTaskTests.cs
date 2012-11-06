using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class AnalyzePostfixLogsTaskTests : IntegrationTestBase
	{
		public AnalyzePostfixLogsTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenCalledAndFoundSentStatusesInTheLog_ShouldRaiseTheHappendOnDeliveryEvent()
		{
			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.Type == MailEventType.Sent);
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "bianca23518@yahoo.com", "a66122s@aol.com", "pkaraszewski@gmail.com" });
															});

		}

		[Test]
		public void Execute_WhenCalledAndFoundBounceStatusesInTheLog_ShouldRaiseTheHappendOnDeliveryEvent()
		{
			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "to=<a11eng@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1, delays=0.04/0/0.67/0.29, dsn=5.1.1, status=bounced (host mailin-03.mx.aol.com[64.12.90.33] said: 550 5.1.1 <a11eng@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = " 5CDF7AE39E: to=<a.villa767@verizon.net>, relay=relay.verizon.net[206.46.232.11]:25, delay=0.55, delays=0.04/0/0.38/0.13, dsn=5.0.0, status=bounced (host relay.verizon.net[206.46.232.11] said: 550 4.2.1 mailbox temporarily disabled: a.villa767@verizon.net (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.Type == MailEventType.Bounced);
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "a11eng@aol.com", "pnc211@gmail.com", "a.villa767@verizon.net" });
															});

		}
	}
}
