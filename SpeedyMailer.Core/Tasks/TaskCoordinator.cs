using System;
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

		public TaskCoordinator(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void BeginExecuting()
		{
			if (!_scheduler.InStandbyMode) return;

			var jobDetail = JobBuilder.Create<StartTaskExecutionJob>()
				.WithIdentity("StartTaskExecution")
				.RequestRecovery()
				.StoreDurably()
				.Build();

			if (!_scheduler.CheckExists(jobDetail.Key))
			{
				_scheduler.AddJob(jobDetail, true);	
			}
			
			_scheduler.TriggerJob(jobDetail.Key);
			_scheduler.Start();
		}
	}

	public class StartTaskExecutionJob:IJob
	{
		private readonly ITaskExecutor _taskExecutor;

		public StartTaskExecutionJob(ITaskExecutor taskExecutor)
		{
			_taskExecutor = taskExecutor;
		}

		public void Execute(IJobExecutionContext context)
		{
			_taskExecutor.Start();
			context.Scheduler.Standby();
		}
	}
}