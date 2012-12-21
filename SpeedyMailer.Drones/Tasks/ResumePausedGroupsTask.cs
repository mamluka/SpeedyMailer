using System;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;
using System.Linq;

namespace SpeedyMailer.Drones.Tasks
{
	public class ResumePausedGroupsTask : ScheduledTask
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

			public Job(OmniRecordManager omniRecordManager, EventDispatcher eventDispatcher)
			{
				_eventDispatcher = eventDispatcher;
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>();

				if (groupsSendingPolicies == null)
					return;

				var resumedDomains = groupsSendingPolicies.GroupSendingPolicies.Where(x => x.Value.ResumeAt < DateTime.UtcNow).Select(x => x.Key).ToList();

				groupsSendingPolicies.GroupSendingPolicies = groupsSendingPolicies
					.GroupSendingPolicies
					.Where(x => x.Value.ResumeAt >= DateTime.UtcNow)
					.ToDictionary(x => x.Key, x => x.Value);

				_eventDispatcher.ExecuteAll(new ResumingGroups { Groups = resumedDomains });

				_omniRecordManager.UpdateOrInsert(groupsSendingPolicies);
			}
		}
	}
}