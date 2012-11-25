using System.Collections.Generic;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class DeliveryRealTimeDecision : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private readonly PauseSpecificSendingJobsCommand _pauseSpecificSendingJobsCommand;
		private readonly OmniRecordManager _omniRecordManager;

		public DeliveryRealTimeDecision(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, PauseSpecificSendingJobsCommand pauseSpecificSendingJobsCommand, OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
			_pauseSpecificSendingJobsCommand = pauseSpecificSendingJobsCommand;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			StopSendingIfIpBlockageFound(data);
		}

		public void Inspect(AggregatedMailDeferred data)
		{
			StopSendingIfIpBlockageFound(data);
		}

		private void StopSendingIfIpBlockageFound<T>(AggregatedMailEvents<T> data) where T : IHasDomainGroup, IHasRelayMessage
		{
			var badBouncesGroups = data
				.MailEvents
				.Select(x =>
							{
								_classifyNonDeliveredMailCommand.Message = x.Message;
								var bounceType = _classifyNonDeliveredMailCommand.Execute();
								return new { BounceType = bounceType, x.DomainGroup };
							})
				.Where(x => x.BounceType == BounceType.IpBlocked)
				.ToList();

			if (!badBouncesGroups.Any())
				return;

			var ipBlockingGroups = _omniRecordManager.GetSingle<IpBlockingGroups>() ?? new IpBlockingGroups();

			ipBlockingGroups.Groups = ipBlockingGroups.Groups ?? new List<string>();
			ipBlockingGroups.Groups.AddRange(badBouncesGroups.Select(x => x.DomainGroup).ToList());

			_omniRecordManager.BatchInsert(new[] { ipBlockingGroups });

			foreach (var badBounce in badBouncesGroups)
			{
				_pauseSpecificSendingJobsCommand.Group = badBounce.DomainGroup;
				_pauseSpecificSendingJobsCommand.Execute();
			}



		}
	}
}