using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class DeliveryRealTimeDecision : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private readonly PauseSpecificSendingJobsCommand _pauseSpecificSendingJobsCommand;
		private readonly OmniRecordManager _omniRecordManager;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;
		private readonly Logger _logger;

		public DeliveryRealTimeDecision(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, PauseSpecificSendingJobsCommand pauseSpecificSendingJobsCommand, OmniRecordManager omniRecordManager, CreativeFragmentSettings creativeFragmentSettings, Logger logger)
		{
			_logger = logger;
			_creativeFragmentSettings = creativeFragmentSettings;
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
				.Where(x => x.MailClassfication.Classification == Classification.Blocked)
				.Where(x => ValidDomainGroupsForPausing(x.DomainGroup))
				.ToList();

			if (!bouncesGroups.Any())
				return;

			_logger.Info("Paused the following domains: {0}", string.Join(",", bouncesGroups.Select(x => x.DomainGroup).ToList()));

			var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>() ?? new GroupsAndIndividualDomainsSendingPolicies();

			groupsSendingPolicies.GroupSendingPolicies = groupsSendingPolicies.GroupSendingPolicies ?? new Dictionary<string, ResumeSendingPolicy>();

			bouncesGroups.ForEach(x =>
										 {
											 if (groupsSendingPolicies.GroupSendingPolicies.ContainsKey(x.DomainGroup))
												 return;

											 groupsSendingPolicies.GroupSendingPolicies[x.DomainGroup] = new ResumeSendingPolicy
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

		private bool ValidDomainGroupsForPausing(string domainGroup)
		{
			return !string.IsNullOrEmpty(domainGroup) && domainGroup != _creativeFragmentSettings.DefaultGroup;
		}
	}
}