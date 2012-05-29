using System;
using Quartz;

namespace SpeedyMailer.Drone.Tasks
{
	public class RegisterDroneWithMasterServiceTask : ScheduledTask
	{
		public override string Name
		{
			get { return "RegisterDroneWithMasterServiceTask"; }
		}

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

		public override ScheduledTaskData GetData()
		{
			throw new NotImplementedException();
		}

		public class Job : IJob
		{
			public void Execute(IJobExecutionContext context)
			{

			}
		}
	}
}