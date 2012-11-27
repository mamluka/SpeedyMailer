using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;

namespace SpeedyMailer.Drones.Events
{
	public class ReinstateRecipientsForSending : IHappendOn<AggregatedMailBounced>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;

		public ReinstateRecipientsForSending(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand)
		{
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
					         });

		}
	}
}