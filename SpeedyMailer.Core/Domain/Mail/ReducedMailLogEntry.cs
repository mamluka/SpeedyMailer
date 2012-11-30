using System;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class ReducedMailLogEntry
	{
		public string Message { get; set; }
		public DateTime Time { get; set; }
		public string Level { get; set; }
	}
}