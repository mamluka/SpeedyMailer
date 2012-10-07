using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class SendCreativePackagesWithIntervalTask:DynamiclyScheduledTaskWithData<SendCreativePackagesWithIntervalTask.Data>
	{
		public class Data:ScheduledTaskData
		{
			public int Interval { get; set; }
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public class Job:JobBase<Data>,IJob
		{
			public void Execute(IJobExecutionContext context)
			{
				
			}
		}
	}
}