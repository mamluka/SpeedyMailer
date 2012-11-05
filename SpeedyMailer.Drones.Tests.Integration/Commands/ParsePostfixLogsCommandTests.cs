using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class ParsePostfixLogsCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenLogsContainingEmailSentData_ShouldParseIt()
		{
			var logEntries = new[] 
				                 {
					               new MailLogEntry { Msg = "to=<bianca23518@yahoo.com>, relay=mta7.am0.yahoodns.net[98.136.216.26]:25, delay=1.7, delays=0.04/0/0.63/1, dsn=2.0.0, status=sent (250 ok dirdel)",Time = DateTime.Now,Level="INFO"}  
				                 };

			DroneActions.Store(logEntries);

			DroneActions.ExecuteCommand<ParsePostfixLogsCommand>();



		}
	}

	public class ParsePostfixLogsCommand : Command<>
	{
		public override void Execute()
		{

		}
	}

	public class MailLogEntry
	{
		public string Msg { get; set; }
		public string Level { get; set; }
		public DateTime Time { get; set; }
	}
}
