using System;

namespace SpeedyMailer.Core.Domain.Mail
{
	public interface IHasTime
	{
		DateTime Time { get; set; }
	}
}