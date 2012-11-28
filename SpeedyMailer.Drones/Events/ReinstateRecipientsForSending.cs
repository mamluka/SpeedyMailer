using System.Linq;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class ReinstateRecipientsForSending : IHappendOn<AggregatedMailBounced>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private CreativePackagesStore _creativePackagesStore;

		public ReinstateRecipientsForSending(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, CreativePackagesStore creativePackagesStore)
		{
			_creativePackagesStore = creativePackagesStore;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var mailBounces = data
				.MailEvents
				.Where(x =>
							 {
								 _classifyNonDeliveredMailCommand.Message = x.Message;
								 var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
								 var bounceType = mailClassfication.BounceType;
								 return bounceType == BounceType.Blocked || bounceType == BounceType.NotClassified;
							 }).Select(x => x.Recipient);


			var creativePackages = _creativePackagesStore.GetByEmail(mailBounces);

			creativePackages.ForEach(x => x.Processed = false);

			creativePackages.ForEach(x => _creativePackagesStore.Save(x));
		}
	}
}