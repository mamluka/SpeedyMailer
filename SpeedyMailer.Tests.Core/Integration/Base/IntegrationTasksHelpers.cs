using System;
using System.Diagnostics;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Raven.Client;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationTasksHelpers
	{
		private readonly IntegrationStoreHelpers _integrationStoreHelpers;

		public IntegrationTasksHelpers(IntegrationStoreHelpers integrationStoreHelpers)
		{
			_integrationStoreHelpers = integrationStoreHelpers;
		}

		public void WaitForTaskToComplete(string taskId, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Load<PersistentTask>(taskId).Status == PersistentTaskStatus.Executed &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			_integrationStoreHelpers.WaitForStoreWithFunction(condition);
		}

		public void AssertTaskIsNotRunning(SendCreativePackagesWithIntervalTask task)
		{
			var schedulerFactory = new StdSchedulerFactory();
			var schedulers = schedulerFactory.AllSchedulers;

			foreach (var job in schedulers.Select(scheduler => JobExists(task, scheduler)).Where(job => job != null))
			{
				NUnit.Framework.Assert.Fail("The job still exist {0}", job.Key);
			}
		}

		private static IJobDetail JobExists(ScheduledTask task, IScheduler scheduler)
		{
			return scheduler.GetJobDetail(task.GetJob().Key);
		}
	}
}
