using System;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenThereWereTooManyRetries : IHappendOn<ResumingGroups>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private readonly MarkDomainsAsProcessedCommand _markDomainsAsProcessedCommand;

		public StopSendingWhenThereWereTooManyRetries(OmniRecordManager omniRecordManager, MarkDomainsAsProcessedCommand markDomainsAsProcessedCommand)
		{
			_markDomainsAsProcessedCommand = markDomainsAsProcessedCommand;
			_omniRecordManager = omniRecordManager;
		}

		public void Inspect(ResumingGroups data)
		{
			var ipReputation = _omniRecordManager.GetSingle<IpReputation>();

			var groupsResumedThreeTimes = ipReputation
				.ResumingHistory
				.Where(x => x.Value.Count(m => m > DateTime.UtcNow.AddDays(-7)) >= 3)
				.Select(x => x.Key)
				.ToList();

			_markDomainsAsProcessedCommand.Domains = groupsResumedThreeTimes;
			_markDomainsAsProcessedCommand.LoggingLine = "We have marked the folling domains {0} as processed, the actual packages marked are {1}";
			_markDomainsAsProcessedCommand.Execute();
		}
	}
}