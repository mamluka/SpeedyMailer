using System;
using System.Collections.Specialized;
using System.Diagnostics;
using NLog;
using Ninject;
using Ninject.Activation;
using Quartz;
using Quartz.Impl;

namespace SpeedyMailer.Core.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{
			var value = Guid.NewGuid().ToString();
			var schedulerFactory = new StdSchedulerFactory(new NameValueCollection { { "quartz.scheduler.instanceName", value } });
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.ListenerManager.AddSchedulerListener(new SchedulerListener());
			scheduler.JobFactory = context.Kernel.Get<ContainerJobFactory>();
			scheduler.Start();

			return scheduler;
		}
	}

	public class SchedulerListener : ISchedulerListener
	{
		public static Logger Logger = LogManager.GetCurrentClassLogger();

		public void JobScheduled(ITrigger trigger)
		{
		}

		public void JobUnscheduled(TriggerKey triggerKey)
		{

		}

		public void TriggerFinalized(ITrigger trigger)
		{

		}

		public void TriggerPaused(TriggerKey triggerKey)
		{

		}

		public void TriggersPaused(string triggerGroup)
		{

		}

		public void TriggerResumed(TriggerKey triggerKey)
		{

		}

		public void TriggersResumed(string triggerGroup)
		{

		}

		public void JobAdded(IJobDetail jobDetail)
		{

		}

		public void JobDeleted(JobKey jobKey)
		{

		}

		public void JobPaused(JobKey jobKey)
		{

		}

		public void JobsPaused(string jobGroup)
		{

		}

		public void JobResumed(JobKey jobKey)
		{

		}

		public void JobsResumed(string jobGroup)
		{

		}

		public void SchedulerError(string msg, SchedulerException cause)
		{
			Logger.ErrorException("A scheduled task has an exception", cause);
		}

		public void SchedulerInStandbyMode()
		{

		}

		public void SchedulerStarted()
		{

		}

		public void SchedulerStarting()
		{
			
		}

		public void SchedulerShutdown()
		{

		}

		public void SchedulerShuttingdown()
		{

		}

		public void SchedulingDataCleared()
		{

		}
	}
}