using System;
using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class MailEvent
	{
		public MailEventType Type { get; set; }
		public MailEventLevel Level { get; set; }
		public DateTime Time { get; set; }
		public string Recipient { get; set; }
		public string RelayHost { get; set; }
		public string RelayIp { get; set; }
		public string TotalDelay { get; set; }
		public IList<double> DelayBreakDown { get; set; }
		public string RelayMessage { get; set; }
	    public string MessageId { get; set; }
	    public string CreaiveId { get; set; }
	}
}