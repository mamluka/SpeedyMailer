using Ninject.Activation;
using Quartz;
using Quartz.Impl;

namespace SpeedyMailer.Shared.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{
			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.JobFactory = new ContainerJobFactory(context.Kernel);
			return scheduler;
		}
	}
}