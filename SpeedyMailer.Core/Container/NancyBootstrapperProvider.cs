using System;
using Nancy.Bootstrapper;
using Ninject;
using Ninject.Activation;

namespace SpeedyMailer.Core.Container
{
	public class NancyBootstrapperProvider : Provider<INancyBootstrapper>
	{
		private readonly Action<IKernel> _action;

		public NancyBootstrapperProvider(Action<IKernel> action)
		{
			_action = action;
		}

		protected override INancyBootstrapper CreateInstance(IContext context)
		{
			return new NancyContainerBootstrapper(_action);
		}
	}
}