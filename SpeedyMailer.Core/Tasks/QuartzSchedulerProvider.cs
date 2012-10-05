using System;
using System.Collections.Specialized;
using Ninject.Activation;
using Quartz;
using Quartz.Impl;

namespace SpeedyMailer.Core.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{
			var schedulerFactory = new StdSchedulerFactory(new NameValueCollection { { "quartz.scheduler.instanceName", Guid.NewGuid().ToString() } });
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.JobFactory = new ContainerJobFactory(context.Kernel);
			scheduler.Start();

			return scheduler;
		}
	}
}