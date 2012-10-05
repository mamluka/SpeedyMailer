using System.Diagnostics;
using Ninject.Activation;
using Quartz;
using Quartz.Impl;
using System.Linq;

namespace SpeedyMailer.Core.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{
			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.JobFactory = new ContainerJobFactory(context.Kernel);
			scheduler.Start();
			return scheduler;
		}
	}
}