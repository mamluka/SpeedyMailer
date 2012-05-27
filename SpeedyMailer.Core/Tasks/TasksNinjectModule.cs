using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace SpeedyMailer.Core.Tasks
{
	public class TasksNinjectModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IScheduler>().ToProvider<QuartzSchedulerProvider>();
		}
	}

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

	public class ContainerJobFactory : IJobFactory
	{
		private readonly IKernel _kernel;

		public ContainerJobFactory(IKernel kernel)
		{
			_kernel = kernel;
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
		{
			var type = bundle.JobDetail.JobType;
			return _kernel.Get(type) as IJob;
		}

	}
}
