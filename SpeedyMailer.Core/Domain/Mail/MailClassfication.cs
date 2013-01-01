using System;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class MailClassfication
	{
		public BounceType BounceType { get; set; }
		public TimeSpan TimeSpan { get; set; }
	}
}