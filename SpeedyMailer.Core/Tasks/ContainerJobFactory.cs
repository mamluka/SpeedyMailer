using Ninject;
using Quartz;
using Quartz.Spi;

namespace SpeedyMailer.Core.Tasks
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