using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public static void JobExistWithData<T>(this IScheduler target)
		{
			var groups = target.GetJobGroupNames();
			var data = groups
				.SelectMany(x => target.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)))
				.Where(x => x.Name.Contains(typeof(T).Name))
				.Select(target.GetJobDetail)
				.Any();
		}
	}
}
