using Ninject.Activation;
using Quartz;
using Quartz.Impl;
using Ninject;
using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Core.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{
			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.JobFactory = new ContainerJobFactory(context.Kernel);
			var api = context.Kernel.Get<Api>();
			return scheduler;
		}
	}
}