using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class ParsePostfixLogsCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenLogsContainingEmailSentData_ShouldParseIt()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = "to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}
				                 };

			var result = DroneActions.ExecuteCommand<ParsePostfixLogsCommand, IList<MailEvent>>(x=> x.Logs = logEntries);

			result.Should().Contain(x => x.Type == MailEventType.Sent &&
										 x.Time == new DateTime(2012, 1, 1, 0, 0, 0) &&
										 x.Level == MailEventLevel.Info &&
										 x.Recipient == "bianca23518@yahoo.com" &&
										 x.RelayHost == "mta7.am0.yahoodns.net" &&
										 x.RelayIp == "98.136.216.26" &&
										 x.TotalDelay == "1.7" &&
										 x.RelayMessage == "250 ok dirdel"
				);

			result[0].DelayBreakDown.Should().Contain(new[] { 0.04, 0, 0.63, 1 });
		}
		
		[Test]
		public void Execute_WhenGivenLogsContainingEmailBouncedData_ShouldParseIt()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = " 59C18AE39B: to=<a.and@comcast.net>, relay=mx2.comcast.net[2001:558:fe2d:70::22]:25, delay=0.95, delays=0.04/0/0.46/0.44, dsn=5.1.1, status=bounced (host mx2.comcast.net[2001:558:fe2d:70::22] said: 550 5.1.1 Not our Customer (in reply to RCPT TO command))", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}
				                 };

			var result = DroneActions.ExecuteCommand<ParsePostfixLogsCommand, IList<MailEvent>>(x=> x.Logs = logEntries);

			result.Should().Contain(x => x.Type == MailEventType.Bounced &&
										 x.Time == new DateTime(2012, 1, 1, 0, 0, 0) &&
										 x.Level == MailEventLevel.Info &&
										 x.Recipient == "a.and@comcast.net" &&
										 x.RelayHost == "mx2.comcast.net" &&
										 x.RelayIp == "2001:558:fe2d:70::22" &&
										 x.TotalDelay == "0.95" &&
										 x.RelayMessage == "host mx2.comcast.net[2001:558:fe2d:70::22] said: 550 5.1.1 Not our Customer (in reply to RCPT TO command)"
				);

			result[0].DelayBreakDown.Should().Contain(new[] { 0.04, 0, 0.46, 0.44 });
		}
		
		[Test]
		public void Execute_WhenGivenLogsContainingEmailDeferredData_ShouldParseIt()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = " 64210AE3A5: to=<aacorley@sbcglobal.net>, relay=mx2.sbcglobal.am0.yahoodns.net[98.136.217.192]:25, delay=2.5, delays=0.04/0/1.9/0.54, dsn=4.0.0, status=deferred (host mx2.sbcglobal.am0.yahoodns.net[98.136.217.192] said: 451 Message temporarily deferred - [160] (in reply to end of DATA command))", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}
				                 };

			var result = DroneActions.ExecuteCommand<ParsePostfixLogsCommand, IList<MailEvent>>(x=> x.Logs = logEntries);

			result.Should().Contain(x => x.Type == MailEventType.Deferred &&
										 x.Time == new DateTime(2012, 1, 1, 0, 0, 0) &&
										 x.Level == MailEventLevel.Info &&
										 x.Recipient == "aacorley@sbcglobal.net" &&
										 x.RelayHost == "mx2.sbcglobal.am0.yahoodns.net" &&
										 x.RelayIp == "98.136.217.192" &&
										 x.TotalDelay == "2.5" &&
										 x.RelayMessage == "host mx2.sbcglobal.am0.yahoodns.net[98.136.217.192] said: 451 Message temporarily deferred - [160] (in reply to end of DATA command)"
				);

			result[0].DelayBreakDown.Should().Contain(new[] { 0.04, 0, 1.9, 0.54 });
		}
		
		[Test]
		public void Execute_WhenGivenLogsContaingOtherDataThenSendData_ShouldNotParseIt()
		{
			var logEntries = new List<MailLogEntry>
				                 {
					                 new MailLogEntry {msg = " connect from localhost.localdomain[127.0.0.1]", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"},
					                 new MailLogEntry {msg = " 67253AE3A7: client=localhost.localdomain[127.0.0.1]", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"},
					                 new MailLogEntry {msg = " 687EFAE3A8: from=<david@xomixinc.com>, size=1098, nrcpt=1 (queue active)", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"},
					                 new MailLogEntry {msg = " 687EFAE3A8: message-id=<20121028125520.687EFAE3A8@mail.xomixinc.com>", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}, 
					                 new MailLogEntry {msg = " 67E3DAE362: removed", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}, 
					                 new MailLogEntry {msg = " disconnect from localhost.localdomain[127.0.0.1]", time = new DateTime(2012, 1, 1, 0, 0, 0), level = "INFO"}, 
				                 };

			var result = DroneActions.ExecuteCommand<ParsePostfixLogsCommand, IList<MailEvent>>(x=> x.Logs = logEntries);

			result.Should().BeEmpty();
		}
	}
}