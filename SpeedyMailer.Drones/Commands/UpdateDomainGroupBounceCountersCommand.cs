using System.Collections.Generic;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Drones.Commands
{
	public class UpdateDomainGroupBounceCountersCommand:Command<IList<MailBounced>>
	{
		public override IList<MailBounced> Execute()
		{
			return null;
		}
	}
}