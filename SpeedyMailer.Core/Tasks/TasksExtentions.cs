using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public static class TasksExtentions
	{
		public static void Stop(this IJob job, IJobExecutionContext context)
		{
			context.Scheduler.DeleteJob(context.JobDetail.Key);
		}
	}
}
