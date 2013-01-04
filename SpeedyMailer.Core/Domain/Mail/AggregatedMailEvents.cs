using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class AggregatedMailEvents<T>
	{
		public IList<T> MailEvents { get; set; }
	}
}