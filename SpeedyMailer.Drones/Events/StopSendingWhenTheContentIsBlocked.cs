using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenTheContentIsBlocked:IHappendOn<AggregatedMailBounced>
	{
		private readonly MarkDomainsAsProcessedCommand _markDomainsAsProcessedCommand;

		public StopSendingWhenTheContentIsBlocked(MarkDomainsAsProcessedCommand markDomainsAsProcessedCommand)
		{
			_markDomainsAsProcessedCommand = markDomainsAsProcessedCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var domains  = data
				.MailEvents
				.GetDomains(Classification.ContentBlocking);

			_markDomainsAsProcessedCommand.Domains = domains;
			_markDomainsAsProcessedCommand.Execute();
		}
	}
}