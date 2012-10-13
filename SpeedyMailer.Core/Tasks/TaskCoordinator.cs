using System;
using NLog;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public interface ITaskCoordinator
	{
		void BeginExecuting();
	}

	public class TaskCoordinator : ITaskCoordinator
	{
		private readonly IScheduler _scheduler;
		private readonly Logger _logger;

		public TaskCoordinator(IScheduler scheduler, Logger logger)
		{
			_logger = logger;
			_scheduler = scheduler;

			_logger.Info("TaskCoordincator started with scheduler: {0}", scheduler.SchedulerInstanceId);
		}

		public void BeginExecuting()
		{
			var job = JobBuilder.Create<StartTaskExecutionJob>()
				.WithIdentity("StartTaskExecution", "Tasks")
				.RequestRecovery()
				.StoreDurably()
				.Build();

			var trigger = TriggerBuilder.Create()
				.WithIdentity("TaskExecutionTrigger")
				.WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
				.StartNow()
				.Build();

			_scheduler.StartIfNeeded();

			if (!_scheduler.CheckExists(job.Key))
			{
				_scheduler.ScheduleJob(job, trigger);
			}
		}
	}

	[DisallowConcurrentExecution]
	public class StartTaskExecutionJob : IJob
	{
		private readonly ITaskExecutor _taskExecutor;

		public StartTaskExecutionJob(ITaskExecutor taskExecutor)
		{
			_taskExecutor = taskExecutor;
		}

		public void Execute(IJobExecutionContext context)
		{
			_taskExecutor.Start();
		}
	}
}