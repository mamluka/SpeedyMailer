using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class AggregatedMailSent
	{
		public IList<MailEvent> MailEvents { get; set; }
	}
}