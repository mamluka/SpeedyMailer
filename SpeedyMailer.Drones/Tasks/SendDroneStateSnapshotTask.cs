using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class SendDroneStateSnapshotTask : ScheduledTask
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
			public void Execute(IJobExecutionContext context)
			{

			}
		}
	}
}