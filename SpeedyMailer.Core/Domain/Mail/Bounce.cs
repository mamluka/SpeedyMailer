using System;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class Bounce
	{
		public string Address { get; set; }

		public string DomainGroup { get; set; }

		public DateTime Time { get; set; }
	}
}