using System;
using System.Linq;
using NLog;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities.Extentions;
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
			private readonly EventDispatcher _eventDispatcher;
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly Logger _logger;

			public Job(OmniRecordManager omniRecordManager, EventDispatcher eventDispatcher, CreativePackagesStore creativePackagesStore, Logger logger)
			{
				_logger = logger;
				_creativePackagesStore = creativePackagesStore;
				_eventDispatcher = eventDispatcher;
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				var sendingPolicies = _omniRecordManager.Load<GroupsAndIndividualDomainsSendingPolicies>();
				if (sendingPolicies == null)
					return;

				var domainToResume = sendingPolicies.
					GroupSendingPolicies
					.Where(x => x.Value.ResumeAt < DateTime.UtcNow)
					.Select(x => x.Key)
					.ToList();

				if (!domainToResume.Any())
					return;

				var packages = _creativePackagesStore.GetByDomains(domainToResume);

				packages
					.ToList()
					.ForEach(x =>
					{
						x.Processed = false;
						_creativePackagesStore.Save(x);
					});

				_logger.Info("Resuming domains for sending: {0} and resuming sending for packages: {1}", domainToResume.Commafy(), packages.Select(x => x.To).Commafy());

				_eventDispatcher.ExecuteAll(new ResumingGroups { Groups = domainToResume });
			}
		}
	}
}