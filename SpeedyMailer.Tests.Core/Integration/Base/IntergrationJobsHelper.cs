using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using FluentAssertions;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl.Matchers;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntergrationJobsHelper
	{
		private readonly IScheduler _masterScheduler;
		private readonly IScheduler _droneScheduler;
		private IScheduler _scheduler;

		public IntergrationJobsHelper(IScheduler masterScheduler, IScheduler droneScheduler)
		{
			_droneScheduler = droneScheduler;
			_masterScheduler = masterScheduler;
		}

		public IntergrationJobsHelper Drone()
		{
			_scheduler = _droneScheduler;
			return this;
		}

		public IntergrationJobsHelper Master()
		{
			_scheduler = _masterScheduler;
			return this;
		}

		public void AssertJobIsCurrentlyRunnnig<T>(Expression<Func<T, bool>> assertFunc)
		{
			var jobKeys = GetJobKeys();
			var data = GetData<T>(jobKeys);

			data.Should().Contain(assertFunc);
		}

		public void AssertJobWasRemoved<T>(Expression<Func<T, bool>> assertFunc)
		{
			var jobKeys = GetJobKeys();
			var data = GetData<T>(jobKeys);

			data.Should().NotContain(assertFunc);
		}

		public void WaitForJobToStart(SendCreativePackagesWithIntervalTask task, int waitFor = 30)
		{
			var st = new Stopwatch();
			st.Start();

			while (st.ElapsedMilliseconds < 30 * 1000 && GetJobKeys().All(x => x.Name != GetFullJobName(task)))
			{
				Thread.Sleep(250);
			}

			GetJobKeys().SingleOrDefault(x => x.Name == GetFullJobName(task)).Should().NotBeNull("Job did not start");
		}

		private static string GetFullJobName(SendCreativePackagesWithIntervalTask task)
		{
			return task.GetNamePrefix() + "Job";
		}

		private IEnumerable<T> GetData<T>(IEnumerable<JobKey> jobKeys)
		{
			return jobKeys
				.Select(x => _scheduler.GetJobDetail(x).JobDataMap)
				.Where(x => x != null && x.Contains("data"))
				.Select(x => JsonConvert.DeserializeObject<T>((string)x["data"]))
				.ToList();
		}

		private IEnumerable<JobKey> GetJobKeys()
		{
			var groups = _scheduler.GetJobGroupNames();
			return groups.SelectMany(x => _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));
		}

		public void AssertJobWasPaused<T>(Expression<Func<T, bool>> assertFunc)
		{
			var jobKeys = GetJobKeys();
			var pausedJobs = jobKeys.Where(x => _scheduler.GetTriggersOfJob(x).Any(trigger => _scheduler.GetTriggerState(trigger.Key) == TriggerState.Paused));

			var data = GetData<T>(pausedJobs);

			data.Should().Contain(assertFunc);
		}
	}
}