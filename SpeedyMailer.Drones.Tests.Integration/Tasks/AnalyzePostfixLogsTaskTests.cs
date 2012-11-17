using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using MongoDB.Driver.Builders;
using Mongol;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Abstractions.Extensions;
using Rhino.Mocks;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Drones.Storage;
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Drones.Tests.Integration.Events;
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
					                 new MailLogEntry {Msg = "to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailSent>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "bianca23518@yahoo.com", "a66122s@aol.com", "pkaraszewski@gmail.com" });
															});

		}

		[Test]
		public void Execute_WhenThereAreManyEntriesInTheLog_ShouldGrabOnlyTheOneThatWasNotProcesses()
		{
			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", Time = new DateTime(2012, 1, 1, 1, 1, 1,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", Time = new DateTime(2012, 1, 1, 3,1, 1,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", Time = new DateTime(2012, 1, 1,3, 3, 1,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailSent>(3);

			EventRegistry.Clear();

			var logEntries2 = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "to=<xbianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", Time = new DateTime(2012, 1, 1, 1, 1, 1,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 6715DAE362: to=<xa66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", Time = new DateTime(2012, 1, 1, 3,1, 1,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " EECBDAE8E7: to=<xpkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", Time = new DateTime(2012, 1, 1,3, 3, 1,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries2, "logs");

			DroneActions.FireExistingTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailSent>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "bianca23518@yahoo.com", "a66122s@aol.com", "pkaraszewski@gmail.com" });
															});

		}

		[Test]
		public void Execute_WhenCalledAndFoundBounceStatusesInTheLog_ShouldRaiseTheHappendOnDeliveryEvent()
		{
			ListenToEvent<AggregatedMailBounced>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "to=<a11eng@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1, delays=0.04/0/0.67/0.29, dsn=5.1.1, status=bounced (host mailin-03.mx.aol.com[64.12.90.33] said: 550 5.1.1 <a11eng@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 5CDF7AE39E: to=<a.villa767@verizon.net>, relay=relay.verizon.net[206.46.232.11]:25, delay=0.55, delays=0.04/0/0.38/0.13, dsn=5.0.0, status=bounced (host relay.verizon.net[206.46.232.11] said: 550 4.2.1 mailbox temporarily disabled: a.villa767@verizon.net (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, collectionName: "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailBounced>(x =>
															   {
																   x.MailEvents.Should().ContainItemsAssignableTo<MailBounced>();
																   x.MailEvents
																	   .Select(mailEvent => mailEvent.Recipient)
																	   .Should()
																	   .BeEquivalentTo(new[] { "a11eng@aol.com", "pnc211@gmail.com", "a.villa767@verizon.net" });
															   });

		}

		[Test]
		public void Execute_WhenCalledAndFoundDeferredStatusInTheLog_ShouldRaiseTheHappendOnDeliveryEvent()
		{
			ListenToEvent<AggregatedMailDeferred>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = " 5CB0EAE39D: to=<aabubars@sbcglobal.net>, relay=mx2.sbcglobal.am0.yahoodns.net[98.136.217.192]:25, delay=2.5, delays=0.12/0/1.9/0.56, dsn=4.0.0, status=deferred (host mx2.sbcglobal.am0.yahoodns.net[98.136.217.192] said: 451 Message temporarily deferred - [160] (in reply to end of DATA command))", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = " B1F58AE39F: to=<lorihooks@5aol.com>, relay=none, delay=36378, delays=36273/0/105/0, dsn=4.4.1, status=deferred (connect to 5aol.com[205.188.101.24]:25: Connection timed out)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 67253AE3A7: to=<a336448@aol.com>, relay=none, delay=0.05, delays=0.04/0/0/0, dsn=4.3.0, status=deferred (mail transport unavailable)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailDeferred>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailDeferred>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "aabubars@sbcglobal.net", "lorihooks@5aol.com", "a336448@aol.com" });
															});

		}

		[Test]
		public void Execute_WhenCalledAndFoundDeferredStatusInTheLog_ShouldOnlyPublshTheDeferredEvent()
		{
			ListenToEvent<AggregatedMailDeferred>();
			ListenToEvent<AggregatedMailBounced>();
			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = " 5CB0EAE39D: to=<aabubars@sbcglobal.net>, relay=mx2.sbcglobal.am0.yahoodns.net[98.136.217.192]:25, delay=2.5, delays=0.12/0/1.9/0.56, dsn=4.0.0, status=deferred (host mx2.sbcglobal.am0.yahoodns.net[98.136.217.192] said: 451 Message temporarily deferred - [160] (in reply to end of DATA command))", Time = new DateTime(2012, 1, 1, 0, 0, 0), Level = "INFO"},
					                 new MailLogEntry {Msg = " B1F58AE39F: to=<lorihooks@5aol.com>, relay=none, delay=36378, delays=36273/0/105/0, dsn=4.4.1, status=deferred (connect to 5aol.com[205.188.101.24]:25: Connection timed out)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
					                 new MailLogEntry {Msg = " 67253AE3A7: to=<a336448@aol.com>, relay=none, delay=0.05, delays=0.04/0/0/0, dsn=4.3.0, status=deferred (mail transport unavailable)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailDeferred>();
			AssertEventWasNotPublished<AggregatedMailBounced>();
			AssertEventWasNotPublished<AggregatedMailSent>();
		}

		[Test]
		public void Execute_WhenStatusLogsfound_ShouldStoreBouncesMails()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");
			DroneActions.Store(new IntervalRule
								   {
									   Conditons = new List<string> { "gmail.com" },
									   Group = "gmail"
								   });

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailBounced>();

			var result = DroneActions.FindAll<MailBounced>().First();

			result.Recipient.Should().Be("pnc211@gmail.com");
			result.DomainGroup.Should().Be("gmail");
			result.Time.Should().Be(new DateTime(2012, 1, 1, 0, 0, 0));
		}

		[Test]
		public void Execute_WhenStatusLogsFoundButThereAreNoIntervalRules_ShouldStoreWithDefaultDomainGroup()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailBounced>();

			var result = DroneActions.FindAll<MailBounced>().First();

			result.Recipient.Should().Be("pnc211@gmail.com");
			result.DomainGroup.Should().Be("$default$");
			result.Time.Should().Be(new DateTime(2012, 1, 1, 0, 0, 0));
		}

		[Test]
		public void Execute_WhenStatusLogsFoundButThereAreNoInervalRules_ShouldStoreSentMails()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");
			DroneActions.Store(new IntervalRule
			{
				Conditons = new List<string> { "aol.com" },
				Group = "aol"
			});

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailSent>();

			var result = DroneActions.FindAll<MailSent>().First();

			result.Recipient.Should().Be("a66122s@aol.com");
			result.DomainGroup.Should().Be("aol");
			result.Time.Should().Be(new DateTime(2012, 1, 1, 0, 0, 0));
		}

		[Test]
		public void Execute_WhenStatusLogsfound_ShouldStoreDeferredMails()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {Msg = " 5CB0EAE39D: to=<aabubars@sbcglobal.net>, relay=mx2.sbcglobal.am0.yahoodns.net[98.136.217.192]:25, delay=2.5, delays=0.12/0/1.9/0.56, dsn=4.0.0, status=deferred (host mx2.sbcglobal.am0.yahoodns.net[98.136.217.192] said: 451 Message temporarily deferred - [160] (in reply to end of DATA command))", Time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), Level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "logs");
			DroneActions.Store(new IntervalRule
			{
				Conditons = new List<string> { "sbcglobal.net" },
				Group = "sbcglobal"
			});

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailDeferred>();

			var result = DroneActions.FindAll<MailDeferred>().First();

			result.Recipient.Should().Be("aabubars@sbcglobal.net");
			result.DomainGroup.Should().Be("sbcglobal");
			result.Time.Should().Be(new DateTime(2012, 1, 1, 0, 0, 0));
		}
	}
}
