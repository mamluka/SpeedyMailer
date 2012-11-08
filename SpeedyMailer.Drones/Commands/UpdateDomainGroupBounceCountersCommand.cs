using System.Collections.Generic;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Drones.Commands
{
	public class UpdateDomainGroupBounceCountersCommand:Command<IList<Bounce>>
	{
		public override IList<Bounce> Execute()
		{
			return null;
		}
	}
}