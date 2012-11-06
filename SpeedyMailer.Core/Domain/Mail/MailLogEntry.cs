using System;
using Mongol;

namespace SpeedyMailer.Core.Domain.Mail
{
	[CollectionName("log")]
	public class MailLogEntry
	{
		public string Msg { get; set; }
		public string Level { get; set; }
		public DateTime Time { get; set; }
	}
}