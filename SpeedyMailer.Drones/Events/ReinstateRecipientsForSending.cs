using System.Linq;
using NLog;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class ReinstateRecipientsForSending : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private readonly CreativePackagesStore _creativePackagesStore;
		private Logger _logger;

		public ReinstateRecipientsForSending(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, CreativePackagesStore creativePackagesStore, Logger logger)
		{
			_logger = logger;
			_creativePackagesStore = creativePackagesStore;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			Reinstate(data);
		}

		public void Inspect(AggregatedMailDeferred data)
		{
			Reinstate(data);
		}

		private void Reinstate<T>(AggregatedMailEvents<T> data) where T : IHasRecipient, IHasRelayMessage
		{
			var mails = data
				.MailEvents
				.Where(x => AreDeferredOrNonClassified(x))
				.Select(x => x.Recipient);


			var creativePackages = _creativePackagesStore.GetByEmail(mails);

			creativePackages.ForEach(x => x.Processed = false);

			_logger.Info("Reinstated the following emails: {0}", string.Join(",", creativePackages.Select(x => x.To)));

			creativePackages.ForEach(x => _creativePackagesStore.Save(x));
		}

		private bool AreDeferredOrNonClassified(IHasRelayMessage x)
		{
			_classifyNonDeliveredMailCommand.Message = x.Message;
			var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
			var bounceType = mailClassfication.BounceType;
			return bounceType == BounceType.Blocked || bounceType == BounceType.NotClassified;
		}
	}
}