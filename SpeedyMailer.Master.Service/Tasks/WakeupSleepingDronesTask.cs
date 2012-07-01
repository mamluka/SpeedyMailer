using System;
using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class WakeupSleepingDronesTask:ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInSeconds(5).RepeatForever());
		}

		public class Job:IJob
		{
			public void Execute(IJobExecutionContext context)
			{
				
			}
		}
	}
}