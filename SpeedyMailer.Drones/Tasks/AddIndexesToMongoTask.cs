using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class AddIndexesToMongoTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInHours(1).WithRepeatCount(1));
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
				_omniRecordManager.EnsureIndex<CreativePackage>(x => x.Group, x => x.Processed);
				_omniRecordManager.EnsureIndex<CreativePackage>(x => x.Processed);
				_omniRecordManager.EnsureIndex<CreativePackage>(x => x.To);
			}
		}
	}
}