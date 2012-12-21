using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class StoreUnclassifiedMailEvents : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;

		public StoreUnclassifiedMailEvents(OmniRecordManager omniRecordManager, ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand)
		{
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
			_omniRecordManager = omniRecordManager;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			StoreUnclassifiedMails(data);
		}

		public void Inspect(AggregatedMailDeferred data)
		{
			StoreUnclassifiedMails(data);
		}

		private void StoreUnclassifiedMails<T>(AggregatedMailEvents<T> data) where T : IHasRecipient, IHasRelayMessage, IHasDomainGroup, IHasTime, IHasCreativeId
		{
			var unclassified = data
				.MailEvents
				.Where(x =>
					{
						_classifyNonDeliveredMailCommand.Message = x.Message;
						var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
						return mailClassfication.BounceType == BounceType.NotClassified;
					})
				.Select(x => new UnclassfiedMailEvent
					{
						CreativeId = x.CreativeId,
						Time = x.Time,
						DomainGroup = x.DomainGroup,
						Message = x.Message,
						Recipient = x.Recipient
					})
				.ToList();

			if (!unclassified.Any())
				return;

			_omniRecordManager.BatchInsert(unclassified);
		}
	}
}