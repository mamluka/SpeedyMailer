using System;
using System.Linq;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
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
			private readonly MarkDomainsAsProcessedCommand _markDomainsAsProcessedCommand;

			public Job(OmniRecordManager omniRecordManager, EventDispatcher eventDispatcher,MarkDomainsAsProcessedCommand markDomainsAsProcessedCommand)
			{
				_markDomainsAsProcessedCommand = markDomainsAsProcessedCommand;
				_eventDispatcher = eventDispatcher;
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

				_markDomainsAsProcessedCommand.Domains = domainToResume;
				_markDomainsAsProcessedCommand.LoggingLine = "Resuming domains for sending: {0} and resuming sending for packages: {1}";
				_markDomainsAsProcessedCommand.Execute();

				_eventDispatcher.ExecuteAll(new ResumingGroups { Groups = domainToResume });
			}
		}
	}
}