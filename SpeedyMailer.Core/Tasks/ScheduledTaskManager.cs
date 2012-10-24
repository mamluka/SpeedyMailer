using System.Collections.Generic;
using NLog;
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
		private readonly Logger _logger;

		public ScheduledTaskManager(IScheduler scheduler, Logger logger)
		{
			_logger = logger;
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

			var trigger = task.GetTrigger();
			_scheduler.ScheduleJob(job, trigger);

			_logger.Info("Job scheduled job name: {0} ,the job will fire at {1}", job.Key, trigger.GetNextFireTimeUtc());

			if (job.JobDataMap.ContainsKey("data"))
				_logger.Info("Job {0} has data: {1}", job.Key, job.JobDataMap["data"]);
		}

		public void AddAndStart(IEnumerable<ScheduledTask> tasks)
		{
			_scheduler.StartIfNeeded();

			foreach (var task in tasks)
			{
				AddAndStart(task);
			}
		}
	}
}