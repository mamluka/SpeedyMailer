using System.Diagnostics;
using Ninject;
using Ninject.Activation;
using Quartz;
using Quartz.Impl;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Tasks
{
	public class QuartzSchedulerProvider : Provider<IScheduler>
	{
		protected override IScheduler CreateInstance(IContext context)
		{

			var a = context.Kernel.Get<NinjectIdentitySettings>();
			Trace.WriteLine("Creating kernel of the scheduler using a provider is: " + a.KernelName);

			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = schedulerFactory.GetScheduler();
			scheduler.JobFactory = new ContainerJobFactory(context.Kernel);
			return scheduler;
		}
	}
}