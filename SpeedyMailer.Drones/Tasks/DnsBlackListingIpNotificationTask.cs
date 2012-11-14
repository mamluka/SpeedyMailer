using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class DnsBlackListingIpNotificationTask:ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(5).RepeatForever());
		}

		public class Job:IJob
		{
			public void Execute(IJobExecutionContext context)
			{
				
			}
		}
	}
}