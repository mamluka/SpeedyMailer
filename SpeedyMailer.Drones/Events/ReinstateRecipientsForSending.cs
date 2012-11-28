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
		private readonly OmniRecordManager _omniRecordManager;
		private MapToCreativePackageCommand _mapToCreativePackageCommand;

		public ReinstateRecipientsForSending(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, OmniRecordManager omniRecordManager,MapToCreativePackageCommand mapToCreativePackageCommand)
		{
			_mapToCreativePackageCommand = mapToCreativePackageCommand;
			_omniRecordManager = omniRecordManager;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var currentExecutingCreativeFragment = _omniRecordManager.GetSingle<CurrentExecutingCreativeFragment>();
			
			if (currentExecutingCreativeFragment == null)
				return;

			var mailBounces = data
				.MailEvents
				.Where(x =>
					         {
						         _classifyNonDeliveredMailCommand.Message = x.Message;
						         var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
						         var bounceType = mailClassfication.BounceType;
						         return bounceType == BounceType.Blocked || bounceType == BounceType.NotClassified;
					         }).ToList();

			
			var creativeFragment = currentExecutingCreativeFragment.CreativeFragment;
		}
	}
}