using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl.Matchers;

namespace SpeedyMailer.Core.Tasks
{
	public static class SchedulerExtentions
	{
		public static void StartIfNeeded(this IScheduler target)
		{
			if (target.IsShutdown)
			{
				target.Start();
			}
		}

		public static bool JobExistWithData<TEventData>(this IScheduler target,Func<TEventData,bool> testFunc)
		{
			return GetCurrentJobs(target)
				.Where(x => x.Name.Contains(typeof(TEventData).Name))
				.Select(target.GetJobDetail)
				.Select(ConvertToEventData<TEventData>)
				.Any(testFunc);
		}

		public static void TriggerTaskByClassName(this IScheduler target,string taskName)
		{
			var key = GetCurrentJobs(target).SingleOrDefault(x => x.Name.Contains(taskName));
			if (key == null)
				return;

			target.TriggerJob(key);
		}

		private static IEnumerable<JobKey> GetCurrentJobs(IScheduler target)
		{
			return target
				.GetJobGroupNames()
				.SelectMany(x => target.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));
		}

		public static bool IsJobsRunning<TJob>(this IScheduler target)
		{
			return GetCurrentJobs(target)
				.Any(x => x.Name.Contains(typeof (TJob).Name));
		}


		private static TEventData ConvertToEventData<TEventData>(IJobDetail x)
		{
			return JsonConvert.DeserializeObject<TEventData>((string)x.JobDataMap["data"]);
		}
	}
}
