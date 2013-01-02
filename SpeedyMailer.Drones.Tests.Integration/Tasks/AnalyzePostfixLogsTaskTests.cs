using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
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
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<bianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE777: to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 6715DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<a66122s@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailSent>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "bianca23518@yahoo.com", "a66122s@aol.com", "pkaraszewski@gmail.com" });
																x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.CreativeId == "creative/1");
															});

		}

		[Test]
		public void Execute_WhenTwoLogsHaveDifferentCreativeIds_ShouldCoupleCreativeWithTheRightMail()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<bianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE777: to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAE8E7: info: header Speedy-Creative-Id: creative/2 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
				{
					x.MailEvents.Should().Contain(mailSent => mailSent.CreativeId == "creative/1" && mailSent.Recipient == "bianca23518@yahoo.com");
					x.MailEvents.Should().Contain(mailSent => mailSent.CreativeId == "creative/2" && mailSent.Recipient == "pkaraszewski@gmail.com");
				});

		}

		[Test]
		public void Execute_WhenCalledAndFoundSentMailToPostFixPostMaster_ShouldFilterItOut()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
				{
					x.StoreHostname = DefaultHostUrl;
					x.Domain = "xomixinc.com";
				});

			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = "B1C9512E19CF: to=<root@xomixinc.com>, relay=local, delay=0.01, delays=0/0.01/0/0, dsn=2.0.0, status=sent (delivered to mailbox)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasNotPublished<AggregatedMailSent>();

		}

		[Test]
		public void Execute_WhenThereAreManyEntriesInTheLog_ShouldGrabOnlyTheOneThatWasNotProcesses()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<bianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE777: to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 6715DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<a66122s@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailSent>(3);

			EventRegistry.Clear();

			var logEntries2 = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xbianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE777: to=<xbianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 6715DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xa66122s@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE362: to=<xa66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xpkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EECBDAE8E7: to=<xpkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries2, "log");

			DroneActions.FireExistingTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailSent>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "xbianca23518@yahoo.com", "xa66122s@aol.com", "xpkaraszewski@gmail.com" });
																x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.CreativeId == "creative/1");
															});

		}

		[Test]
		public void Execute_WhenWeCantFindTheMailEventForACreativeIdHeaderEven_ShouldNotMakeThatCreativeHeaderLogAsProcessed()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<AggregatedMailSent>();

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<bianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 1, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE777: to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 1, 1, 2, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 6715DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<a66122s@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1, 3, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 1, 1,4, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 1, 1,5, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailSent>(3);

			EventRegistry.Clear();

			var logEntries2 = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = " EECBDAE8E7: to=<pkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 1, 1, 6, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " DFREAE777: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xbianca23518@yahoo.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " DFREAE777: to=<xbianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 1115DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xa66122s@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 1115DAE362: to=<xa66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EECBDAEQWE: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<xpkaraszewski@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EECBDAEQWE: to=<xpkaraszewski@gmail.com>, relay=gmail-smtp-in.l.google.com[173.194.65.27]:25, delay=0.53, delays=0.04/0/0.06/0.42, dsn=2.0.0, status=sent (250 2.0.0 OK 1351377742 f7si8928630eeo.82)", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries2, "log");

			DroneActions.FireExistingTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents.Should().ContainItemsAssignableTo<MailSent>();
																x.MailEvents
																	.Select(mailEvent => mailEvent.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "pkaraszewski@gmail.com", "xbianca23518@yahoo.com", "xa66122s@aol.com", "xpkaraszewski@gmail.com" });
																x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.CreativeId == "creative/1");
															});

		}

		[Test]
		public void Execute_WhenCalledAndFoundBounceStatusesInTheLog_ShouldRaiseTheHappendOnDeliveryEvent()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			ListenToEvent<AggregatedMailBounced>();

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " PFGMDADWE7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<a11eng@aol.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " PFGMDADWE7: to=<a11eng@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1, delays=0.04/0/0.67/0.29, dsn=5.1.1, status=bounced (host mailin-03.mx.aol.com[64.12.90.33] said: 550 5.1.1 <a11eng@aol.com>: Recipient address rejected: aol.com (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " EF7B3AE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},

                                     new MailLogEntry {msg = " 5CDF7AE39E: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<a.villa767@verizon.net> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 5CDF7AE39E: to=<a.villa767@verizon.net>, relay=relay.verizon.net[206.46.232.11]:25, delay=0.55, delays=0.04/0/0.38/0.13, dsn=5.0.0, status=bounced (host relay.verizon.net[206.46.232.11] said: 550 4.2.1 mailbox temporarily disabled: a.villa767@verizon.net (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, collectionName: "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailBounced>(x =>
															   {
																   x.MailEvents.Should().ContainItemsAssignableTo<MailBounced>();
																   x.MailEvents
																	   .Select(mailEvent => mailEvent.Recipient)
																	   .Should()
																	   .BeEquivalentTo(new[] { "a11eng@aol.com", "pnc211@gmail.com", "a.villa767@verizon.net" });
																   x.MailEvents.Should().OnlyContain(mailEvent => mailEvent.CreativeId == "creative/1");
															   });

		}

		[Test]
		public void Execute_WhenStatusLogsFoundButThereAreNoIntervalRules_ShouldStoreWithDefaultDomainGroup()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " EF7B3AE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailBounced>();

			var result = DroneActions.FindAll<MailBounced>().First();

			result.Recipient.Should().Be("pnc211@gmail.com");
			result.DomainGroup.Should().Be("$default$");
			result.Time.Should().Be(new DateTime(2012, 1, 1, 0, 0, 0));
			result.CreativeId.Should().Be("creative/1");
		}

		[Test]
		public void Execute_WhenStatusLogsfound_ShouldStoreTheDomain()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.StoreHostname = DefaultHostUrl;
															 x.Domain = "xomixinc.com";
														 });

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " EF7B3AE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailBounced>();

			var result = DroneActions.FindAll<MailBounced>().First();

			result.Domain.Should().Be("gmail.com");
		}

		[Test]
		public void Execute_WhenBounceFoundAndCanBeClassified_ShouldStoreTheClassification()
		{
			DroneActions.EditSettings<DroneSettings>(x =>
														 {
															 x.StoreHostname = DefaultHostUrl;
															 x.Domain = "xomixinc.com";
														 });

			DroneActions.Store(new DeliverabilityClassificationRules
				                   {
					                   Rules = new List<HeuristicRule>
						                           {
							                           new HeuristicRule {Condition = "The email account that you tried to reach does not exist", Type = Classification.HardBounce}
						                           }
				                   });

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = "EF7B3AE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			DroneActions.WaitForDocumentToExist<MailBounced>();

			var result = DroneActions.FindAll<MailBounced>().First();

			result.Classification.Classification.Should().Be(Classification.HardBounce);
		}

		[Test]
		public void Execute_WhenStatusLogsfound_ShouldStoreBouncedEmail()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			var logEntries = new List<MailLogEntry>
			    {
				    new MailLogEntry {msg = " EF7B3AE8E7: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				    new MailLogEntry {msg = "EF7B3AE8E7: to=<pnc211@gmail.com>, relay=gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a]:25, delay=3.2, delays=0.04/0/0.11/3.1, dsn=5.1.1, status=bounced (host gmail-smtp-in.l.google.com[2a00:1450:4013:c00::1a] said: 550-5.1.1 The email account that you tried to reach does not exist. Please try 550-5.1.1 double-checking the recipient's email address for typos or 550-5.1.1 unnecessary spaces. Learn more at 550 5.1.1 http://support.google.com/mail/bin/answer.py?answer=6596 f44si10015048eep.23 (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
			    };

			DroneActions.StoreCollection(logEntries, "log");
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
			result.CreativeId.Should().Be("creative/1");
		}

		[Test]
		public void Execute_WhenStatusLogsFound_ShouldStoreSentMails()
		{
			DroneActions.EditSettings<DroneSettings>(x => x.StoreHostname = DefaultHostUrl);

			var logEntries = new List<MailLogEntry>
				                 {
                                     new MailLogEntry {msg = " 6715DAE362: info: header Speedy-Creative-Id: creative/1 from localhost.localdomain[127.0.0.1]; from=<david@xomixinc.com> to=<pnc211@gmail.com> proto=ESMTP helo=<mail>", time = new DateTime(2012, 2, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
					                 new MailLogEntry {msg = " 6715DAE362: to=<a66122s@aol.com>, relay=mailin-03.mx.aol.com[64.12.90.33]:25, delay=1.8, delays=0.04/0/1/0.73, dsn=2.0.0, status=sent (250 2.0.0 Ok: queued as 3E373380000BC)", time = new DateTime(2012, 1, 1, 0, 0, 0,DateTimeKind.Utc), level = "INFO"},
				                 };

			DroneActions.StoreCollection(logEntries, "log");
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
			result.CreativeId.Should().Be("creative/1");
		}
	}
}
