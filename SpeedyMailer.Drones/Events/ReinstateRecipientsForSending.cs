using System.Linq;
using NLog;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class ReinstateRecipientsForSending : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private readonly CreativePackagesStore _creativePackagesStore;
		private readonly Logger _logger;

		public ReinstateRecipientsForSending(CreativePackagesStore creativePackagesStore, Logger logger)
		{
			_logger = logger;
			_creativePackagesStore = creativePackagesStore;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			Reinstate(data);
		}

		public void Inspect(AggregatedMailDeferred data)
		{
			Reinstate(data);
		}

		private void Reinstate<T>(AggregatedMailEvents<T> data) where T : IHasRecipient, IHasRelayMessage,IHasClassification
		{
			var mails = data
				.MailEvents
				.Where(x => x.Classification.Classification == Classification.NotClassified)
				.Select(x => x.Recipient);


			var creativePackages = _creativePackagesStore.GetByEmail(mails);

			creativePackages.ForEach(x => x.Processed = false);

			_logger.Info("Reinstated the following emails: {0}", string.Join(",", creativePackages.Select(x => x.To)));

			creativePackages.ForEach(x => _creativePackagesStore.Save(x));
		}
	}
}