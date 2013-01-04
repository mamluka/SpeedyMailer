using System;
using System.Collections.Generic;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class PauseSendingForIndividualDomains : IHappendOn<AggregatedMailBounced>
	{
		private readonly CreativeFragmentSettings _creativeFragmentSettings;
		private readonly OmniRecordManager _omniRecordManager;
		private readonly EventDispatcher _eventDispatcher;
		private readonly MarkDomainsAsProcessedCommand _markDomainsAsProcessedCommand;

		public PauseSendingForIndividualDomains(OmniRecordManager omniRecordManager,
			CreativeFragmentSettings creativeFragmentSettings,
			EventDispatcher eventDispatcher,
			MarkDomainsAsProcessedCommand markDomainsAsProcessedCommand)
		{
			_markDomainsAsProcessedCommand = markDomainsAsProcessedCommand;
			_eventDispatcher = eventDispatcher;
			_omniRecordManager = omniRecordManager;
			_creativeFragmentSettings = creativeFragmentSettings;
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

			var domains = domainToUndeliver.Select(x => x.Domain).ToList();
			_eventDispatcher.ExecuteAll(new BlockingGroups { Groups = domains });

			_markDomainsAsProcessedCommand.Domains = domains;
			_markDomainsAsProcessedCommand.LoggingLine = "Paused the following domains: {0} and paused the following packages: {1}";
			_markDomainsAsProcessedCommand.Execute();

			var sendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>() ?? NewGroupsAndIndividualDomainsSendingPolicies();

			domainToUndeliver.ForEach(x =>
				{
					sendingPolicies.GroupSendingPolicies[x.Domain] = new ResumeSendingPolicy { ResumeAt = DateTime.UtcNow + x.TimeSpan };
				});

			_omniRecordManager.UpdateOrInsert(sendingPolicies);
		}

		private static GroupsAndIndividualDomainsSendingPolicies NewGroupsAndIndividualDomainsSendingPolicies()
		{
			return new GroupsAndIndividualDomainsSendingPolicies
				{
					GroupSendingPolicies = new Dictionary<string, ResumeSendingPolicy>()
				};
		}
	}
}