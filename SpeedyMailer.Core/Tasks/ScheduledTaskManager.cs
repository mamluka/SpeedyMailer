using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public interface IScheduledTaskManager
	{
		void AddAndStart(ScheduledTask task);
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
			var job = task.GetJob();
			_scheduler.ScheduleJob(job, task.GetTrigger());

			if (!_scheduler.IsStarted)
				_scheduler.Start();
		}
	}
}