using System;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Quartz;

namespace SpeedyMailer.Core.Container
{
	public class NancyContainerBootstrapper : NinjectNancyBootstrapper
	{
		private readonly Action<IKernel> _action;
		private IScheduler _scheduler;

		public NancyContainerBootstrapper(Action<IKernel> action, IScheduler scheduler)
		{
			_scheduler = scheduler;
			_action = action;
		}

		protected override void ConfigureApplicationContainer(IKernel existingContainer)
		{
			_action(existingContainer);
			existingContainer.Rebind<IScheduler>().ToConstant(_scheduler);
		}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration
					.WithOverrides(c => c.Serializers.Insert(0, typeof(NancyJsonNetSerializer)))
					.WithIgnoredAssembly(asm => !asm.FullName.StartsWith("SpeedyMailer", StringComparison.InvariantCultureIgnoreCase));
			}
		}
	}
}