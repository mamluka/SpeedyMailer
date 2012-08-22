using System;
using Nancy.Bootstrappers.Ninject.Mono;
using Ninject;

namespace SpeedyMailer.Master.Service.Container
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
    }
}