using System;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class TurnIntoBouncedEmail : IHappendOn<ResumingGroups>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private CreativePackagesStore _creativePackagesStore;

		public TurnIntoBouncedEmail(OmniRecordManager omniRecordManager, CreativePackagesStore creativePackagesStore)
		{
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