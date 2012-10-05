using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

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
	}
}
