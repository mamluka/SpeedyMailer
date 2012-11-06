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

			DroneActions.StoreCollection(logEntries);

			var task = new AnalyzePostfixLogsTask();

			DroneActions.StartScheduledTask(task);

			AssertEventWasPublished<AggregatedMailSent>(x =>
															{
																x.MailEvents
																	.Select(m => m.Recipient)
																	.Should()
																	.BeEquivalentTo(new[] { "bianca23518@yahoo.com", "a66122s@aol.com", "pkaraszewski@gmail.com" });
															});

		}
	}
}
