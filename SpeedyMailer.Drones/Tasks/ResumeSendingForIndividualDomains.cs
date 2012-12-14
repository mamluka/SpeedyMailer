using System;
using System.Linq;
using NLog;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class ResumeSendingForIndividualDomains : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(30).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly OmniRecordManager _omniRecordManager;
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly Logger _logger;

			public Job(OmniRecordManager omniRecordManager, CreativePackagesStore creativePackagesStore, Logger logger)
			{
				_logger = logger;
				_creativePackagesStore = creativePackagesStore;
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				var sendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>();
				if (sendingPolicies == null)
					return;

				var domainToResume = sendingPolicies.
					GroupSendingPolicies
					.Where(x => x.Value.ResumeAt < DateTime.UtcNow)
					.Select(x => x.Key)
					.ToList();

				if (!domainToResume.Any())
					return;

				_logger.Info("Resuming domains for sending: {0}", string.Join(",", domainToResume));

				var matchedPackages = _creativePackagesStore.GetByDomains(domainToResume);

				_logger.Info("Resuming sending for packages: {0}", string.Join(",", matchedPackages.Select(x => x.To).ToList()));

				matchedPackages
					.ToList()
					.ForEach(x =>
						{
							x.Processed = false;
							_creativePackagesStore.Save(x);
						});
			}
		}
	}
}