using System.Collections.Generic;
using Quartz;
using Quartz.Listener;

namespace SpeedyMailer.Drone.Tasks
{
	public interface IScheduledTaskManager
	{
		void Start(ScheduledTask task);
	}

	public class ScheduledTaskManager : IScheduledTaskManager
	{
		private readonly IScheduler _scheduler;

		public ScheduledTaskManager(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void Start(ScheduledTask task)
		{
			var job = task.GetJob();
			_scheduler.ScheduleJob(job, task.GetTrigger());

			if (!_scheduler.IsStarted)
				_scheduler.Start();
		}
	}
}