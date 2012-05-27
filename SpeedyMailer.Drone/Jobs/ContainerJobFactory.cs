using System;
using Ninject;
using Quartz;
using Quartz.Spi;
using SpeedyMailer.Master.Web.UI.Bootstrappers;

namespace SpeedyMailer.Master.Web.UI.Jobs
{
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