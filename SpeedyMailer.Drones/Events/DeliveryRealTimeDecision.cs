using System;
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
			var bouncesGroups = data
				.MailEvents
				.Select(x =>
							{
								_classifyNonDeliveredMailCommand.Message = x.Message;
								var mailClassfication = _classifyNonDeliveredMailCommand.Execute();
								return new { MailClassfication = mailClassfication, x.DomainGroup };
							})
				.Where(x => x.MailClassfication.BounceType == BounceType.Blocked)
				.Where(x => !string.IsNullOrEmpty(x.DomainGroup))
				.ToList();

			if (!bouncesGroups.Any())
				return;

			var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsSendingPolicies>() ?? new GroupsSendingPolicies();

			groupsSendingPolicies.GroupSendingPolicies = groupsSendingPolicies.GroupSendingPolicies ?? new Dictionary<string, GroupSendingPolicy>();

			bouncesGroups.ForEach(x =>
										 {
											 if (groupsSendingPolicies.GroupSendingPolicies.ContainsKey(x.DomainGroup))
												 return;

											 groupsSendingPolicies.GroupSendingPolicies[x.DomainGroup] = new GroupSendingPolicy
																								{
																									ResumeAt = DateTime.UtcNow + x.MailClassfication.TimeSpan
																								};
										 });

			_omniRecordManager.UpdateOrInsert(groupsSendingPolicies);

			foreach (var badBounce in bouncesGroups)
			{
				_pauseSpecificSendingJobsCommand.Group = badBounce.DomainGroup;
				_pauseSpecificSendingJobsCommand.Execute();
			}
		}
	}
}