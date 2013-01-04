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
	public class PauseSendingForIndividualDomains : IHappendOn<AggregatedMailBounced>
	{
		private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;
		private readonly OmniRecordManager _omniRecordManager;
		private readonly CreativePackagesStore _creativePackagesStore;
		private readonly Logger _logger;
		private readonly EventDispatcher _eventDispatcher;

		public PauseSendingForIndividualDomains(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, OmniRecordManager omniRecordManager, CreativePackagesStore creativePackagesStore, CreativeFragmentSettings creativeFragmentSettings, EventDispatcher eventDispatcher, Logger logger)
		{
			_eventDispatcher = eventDispatcher;
			_logger = logger;
			_creativePackagesStore = creativePackagesStore;
			_omniRecordManager = omniRecordManager;
			_creativeFragmentSettings = creativeFragmentSettings;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			UndeliverabilityDecision(data);
		}

		private void UndeliverabilityDecision<T>(AggregatedMailEvents<T> data) where T : IHasDomainGroup, IHasRecipient, IHasRelayMessage, IHasClassification, IHasDomain
		{
			var domainToUndeliver = data
				.MailEvents
				.Where(x => x.DomainGroup == _creativeFragmentSettings.DefaultGroup)
				.Where(x => x.Classification.Type == Classification.TempBlock)
				.GroupBy(x => x.Domain)
				.Select(x => new { x.First().Classification.TimeSpan, Domain = x.Key })
				.ToList();


			if (!domainToUndeliver.Any())
				return;

			var groupsAndDomains = domainToUndeliver.Select(x => x.Domain).ToList();

			_logger.Info("Paused the following domains: {0}", string.Join(",", groupsAndDomains));

			_eventDispatcher.ExecuteAll(new BlockingGroups { Groups = groupsAndDomains });

			var creativePackagesToUndeliver = _creativePackagesStore.GetByDomains(groupsAndDomains);

			_logger.Info("Paused the following packages: {0}", string.Join(",", creativePackagesToUndeliver.Select(x => x.To).ToList()));

			creativePackagesToUndeliver.ToList().ForEach(x =>
															 {
																 x.Processed = true;
																 _creativePackagesStore.Save(x);
															 });

			var sendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>() ?? NewGroupsAndIndividualDomainsSendingPolicies();

			domainToUndeliver.ForEach(x =>
				{
					sendingPolicies.GroupSendingPolicies[x.Domain] = new ResumeSendingPolicy { ResumeAt = DateTime.UtcNow + x.TimeSpan };
				});

			_omniRecordManager.UpdateOrInsert(sendingPolicies);
		}

		private static GroupsAndIndividualDomainsSendingPolicies NewGroupsAndIndividualDomainsSendingPolicies()
		{
			return new GroupsAndIndividualDomainsSendingPolicies()
				{
					GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>()
				};
		}
	}
}