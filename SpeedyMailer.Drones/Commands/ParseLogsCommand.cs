using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Commands
{
	public class ParseLogsCommand : Command<IList<MailEvent>>
	{
		private readonly DroneSettings _droneSettings;
		private Logger _logger;

		public IList<MailLogEntry> Logs { get; set; }

		public ParseLogsCommand(DroneSettings droneSettings, Logger logger)
		{
			_logger = logger;
			_droneSettings = droneSettings;
		}

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

			var msg = mailLogEntry.msg;
			var mailEvent = new MailEvent
								{
									Level = TryParse(mailLogEntry),
									Recipient = ParseRegexWithOneGroup(msg, "to=<(.+?)>"),
									RelayHost = ParseRegexWithMiltipleGroup(msg, @"relay=(.+?)(\[(.+?)\]:\d{0,2})?,", 1),
									RelayIp = ParseRegexWithMiltipleGroup(msg, @"relay=(.+?)(\[(.+?)\]:\d{0,2})?,", 3),
									Time = mailLogEntry.time,
									Type = ParseType(mailLogEntry.msg),
									DelayBreakDown = ParseDelayBreakdown(mailLogEntry.msg),
									TotalDelay = ParseRegexWithOneGroup(msg, "delay=(.+?),"),
									RelayMessage = ParseRegexWithOneGroup(msg, @"status.+?\((.+?)\)$"),
									MessageId = ParseRegexWithOneGroup(mailLogEntry.msg, @"^\s?(.+?):\s")
								};

			if (string.IsNullOrEmpty(mailEvent.Recipient) || mailEvent.Recipient.Contains(_droneSettings.Domain))
				return null;

			return mailEvent;
		}

		private bool ThisMailLogEntryDoesntHaveSendingInformation(MailLogEntry mailLogEntry)
		{
			return !Regex.Match(mailLogEntry.msg, "status=").Success;
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
			double result;
			if (double.TryParse(ParseRegexWithMiltipleGroup(msg, "delays=(\\d*?\\.?\\d*?)/(\\d*\\.?\\d+?)/(\\d*\\.?\\d+?)/(\\d*\\.?\\d+?),", groupId), out result))
				return result;

			_logger.Info("Was unable to parse delay from messsage", msg);
			return 0;
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
			Enum.TryParse(mailLogEntry.level, true, out level);
			return level;
		}
	}
}