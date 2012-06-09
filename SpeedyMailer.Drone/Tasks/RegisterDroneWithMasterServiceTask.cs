using System;
using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drone.Tasks
{
	public class RegisterDroneWithMasterServiceTask : ScheduledTask
	{
		public override IJobDetail GetJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger GetTrigger()
		{
			return TriggerWithTimeCondition(x =>
											x.WithIntervalInMinutes(1).RepeatForever()
				);
		}

		public class Job : IJob
		{
			public void Execute(IJobExecutionContext context)
			{

			}
		}
	}
}