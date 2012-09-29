using System;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;

namespace SpeedyMailer.Core.Container
{
    public class NancyContainerBootstrapper : NinjectNancyBootstrapper
    {
    	private readonly Action<IKernel> _action;

    	public NancyContainerBootstrapper(Action<IKernel> action)
    	{
    		_action = action;
    	}

    	protected override void ConfigureApplicationContainer(IKernel existingContainer)
    	{
    		_action(existingContainer);
    	}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration.WithOverrides(
					   c => c.Serializers.Insert(0, typeof(JsonNetSerializer)));
			}
		}
    }
}