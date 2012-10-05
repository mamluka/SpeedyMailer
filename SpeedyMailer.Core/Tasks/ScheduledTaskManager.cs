using System.Collections.Generic;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public interface IScheduledTaskManager
	{
		void AddAndStart(ScheduledTask task);
		void AddAndStart(IEnumerable<ScheduledTask> tasks);
	}

	public class ScheduledTaskManager : IScheduledTaskManager
	{
		private readonly IScheduler _scheduler;

		public ScheduledTaskManager(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void AddAndStart(ScheduledTask task)
		{
			_scheduler.StartIfNeeded();

			var job = task.GetJob();

			if (_scheduler.CheckExists(job.Key))
			{
				_scheduler.DeleteJob(job.Key);
			}

			_scheduler.ScheduleJob(job, task.GetTrigger());
		}

		public void AddAndStart(IEnumerable<ScheduledTask> tasks)
		{
			_scheduler.StartIfNeeded();

			foreach (var task in tasks)
			{
				var job = task.GetJob();

				if (_scheduler.CheckExists(job.Key))
				{
					_scheduler.DeleteJob(job.Key);
				}
				_scheduler.ScheduleJob(job, task.GetTrigger());
			}
		}
	}
}