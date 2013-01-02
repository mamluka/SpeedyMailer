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
	public class DeliveryRealTimeDecision : IHappendOn<AggregatedMailBounced>
	{
		private readonly PauseSpecificSendingJobsCommand _pauseSpecificSendingJobsCommand;
		private readonly OmniRecordManager _omniRecordManager;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;
		private readonly Logger _logger;

		public DeliveryRealTimeDecision(PauseSpecificSendingJobsCommand pauseSpecificSendingJobsCommand, OmniRecordManager omniRecordManager, CreativeFragmentSettings creativeFragmentSettings, Logger logger)
		{
			_logger = logger;
			_creativeFragmentSettings = creativeFragmentSettings;
			_omniRecordManager = omniRecordManager;
			_pauseSpecificSendingJobsCommand = pauseSpecificSendingJobsCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			StopSendingIfIpBlockageFound(data);
		}

		private void StopSendingIfIpBlockageFound<T>(AggregatedMailEvents<T> data) where T : IHasDomainGroup, IHasRelayMessage, IHasClassification
		{
			var bouncesGroups = data
				.MailEvents
				.Where(x => x.Classification.Classification == Classification.TempBlock)
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
																									ResumeAt = DateTime.UtcNow + x.Classification.TimeSpan
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