using System;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
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

			public Job(OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsAndIndividualDomainsSendingPolicies>();

				if (groupsSendingPolicies == null)
					return;

				groupsSendingPolicies.GroupSendingPolicies = groupsSendingPolicies
					.GroupSendingPolicies
					.Where(x => x.Value.ResumeAt >= DateTime.UtcNow)
					.ToDictionary(x => x.Key, x => x.Value);

				_omniRecordManager.UpdateOrInsert(groupsSendingPolicies);
			}
		}
	}
}