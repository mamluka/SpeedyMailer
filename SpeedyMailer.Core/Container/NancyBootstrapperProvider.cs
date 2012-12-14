using System;
using Nancy.Bootstrapper;
using Ninject;
using Ninject.Activation;
using Quartz;

namespace SpeedyMailer.Core.Container
{
	public class NancyBootstrapperProvider : Provider<INancyBootstrapper>
	{
		private readonly Action<IKernel> _action;
		private readonly IScheduler _scheduler;

		public NancyBootstrapperProvider(Action<IKernel> action, IScheduler scheduler)
		{
			_scheduler = scheduler;
			_action = action;
		}

		protected override INancyBootstrapper CreateInstance(IContext context)
		{
			return new NancyContainerBootstrapper(_action,_scheduler);
		}
	}
}