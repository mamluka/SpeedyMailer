using System;
using System.Linq;
using NLog;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenThereWereTooManyRetries : IHappendOn<ResumingGroups>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private readonly CreativePackagesStore _creativePackagesStore;
		private Logger _logger;

		public StopSendingWhenThereWereTooManyRetries(OmniRecordManager omniRecordManager, CreativePackagesStore creativePackagesStore, Logger logger)
		{
			_logger = logger;
			_creativePackagesStore = creativePackagesStore;
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

			var packages = _creativePackagesStore.GetByDomains(groupsResumedThreeTimes);

			_logger.Info("We have marked the folling domains {0} as processed, the actual packages marked are {1}",
			             string.Join(",", groupsResumedThreeTimes),
			             string.Join(",", packages.Select(x => x.To)));

			packages
				.ToList()
				.ForEach(x =>
					{
						x.Processed = true;
						_creativePackagesStore.Save(x);
					});
		}
	}
}