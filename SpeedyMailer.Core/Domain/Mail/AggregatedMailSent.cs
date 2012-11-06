using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class AggregatedMail
	{
		public IList<MailEvent> MailEvents { get; set; }
	}

	public class AggregatedMailSent : AggregatedMail
	{

	}

	public class AggregatedMailDeferred : AggregatedMail
	{
	}

	public class AggregatedMailBounced : AggregatedMail
	{
	}
}