using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Drones.Commands
{
	public class ParsePostfixLogsCommand : Command<IList<MailEvent>>
	{
		public IList<MailLogEntry> Logs { get; set; }

		public override IList<MailEvent> Execute()
		{
			return Logs.Select(Parse)
				.Where(x => x != null)
				.ToList();
		}

		private MailEvent Parse(MailLogEntry mailLogEntry)
		{
			if (ThisMailLogEntryDoesntHaveSendingInformation(mailLogEntry))
				return null;

			var msg = mailLogEntry.Msg;
			return new MailEvent
					   {
						   Level = TryParse(mailLogEntry),
						   Recipient = ParseRegexWithOneGroup(msg, "to=<(.+?)>"),
						   RelayHost = ParseRegexWithMiltipleGroup(msg, "relay=(.+?)(\\[(.+?)\\]:\\d{0,2})?,", 1),
						   RelayIp = ParseRegexWithMiltipleGroup(msg, "relay=(.+?)(\\[(.+?)\\]:\\d{0,2})?,", 3),
						   Time = mailLogEntry.Time,
						   Type = ParseType(mailLogEntry.Msg),
						   DelayBreakDown = ParseDelayBreakdown(mailLogEntry.Msg),
						   TotalDelay = ParseRegexWithOneGroup(msg, "delay=(.+?),"),
						   RelayMessage = ParseRegexWithOneGroup(msg, "status.+?\\((.+?)\\)$")
					   };
		}

		private bool ThisMailLogEntryDoesntHaveSendingInformation(MailLogEntry mailLogEntry)
		{
			return !Regex.Match(mailLogEntry.Msg, "status=").Success;
		}

		private IList<double> ParseDelayBreakdown(string msg)
		{
			return new[]
				       {
					       ParseDelaysArrayItem(msg, 1),
					       ParseDelaysArrayItem(msg, 2),
					       ParseDelaysArrayItem(msg, 3),
					       ParseDelaysArrayItem(msg, 4)
				       };
		}

		private double ParseDelaysArrayItem(string msg, int groupId)
		{
			return double.Parse(ParseRegexWithMiltipleGroup(msg, "delays=(\\d*?\\.?\\d*?)/(\\d*\\.?\\d+?)/(\\d*\\.?\\d+?)/(\\d*\\.?\\d+?),", groupId));
		}

		private MailEventType ParseType(string msg)
		{
			MailEventType type;
			Enum.TryParse(ParseRegexWithOneGroup(msg, "status=(.+?)\\s"), true, out type);
			return type;
		}

		private string ParseRegexWithMiltipleGroup(string msg, string pattern, int groupId)
		{
			var match = Regex.Match(msg, pattern);
			return match.Success ? match.Groups[groupId].Value : "";
		}

		private string ParseRegexWithOneGroup(string msg, string pattern)
		{
			return ParseRegexWithMiltipleGroup(msg, pattern, 1);
		}

		private static MailEventLevel TryParse(MailLogEntry mailLogEntry)
		{
			MailEventLevel level;
			Enum.TryParse(mailLogEntry.Level, true, out level);
			return level;
		}
	}
}