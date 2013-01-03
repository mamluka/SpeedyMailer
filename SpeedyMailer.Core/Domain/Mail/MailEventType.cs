using System;

namespace SpeedyMailer.Core.Domain.Mail
{
	[Flags]
	public enum MailEventType
	{
		Sent,
		Bounced,
		Deferred
	}
}