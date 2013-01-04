using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenTheIpIsBlocked : IHappendOn<AggregatedMailBounced>
	{
		private readonly MarkDomainsAsProcessedCommand _markDomainsAsProcessedCommand;

		public StopSendingWhenTheIpIsBlocked(MarkDomainsAsProcessedCommand markDomainsAsProcessedCommand)
		{
			_markDomainsAsProcessedCommand = markDomainsAsProcessedCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var domainsToStop = data
				.MailEvents
				.GetDomains(Classification.IpBlocking);

			_markDomainsAsProcessedCommand.Domains = domainsToStop;
			_markDomainsAsProcessedCommand.Execute();
		}
	}
}