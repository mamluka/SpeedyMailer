using NLog;
using Ninject;
using Quartz;
using Quartz.Spi;

namespace SpeedyMailer.Core.Tasks
{
	public class ContainerJobFactory : IJobFactory
	{
		private readonly IKernel _kernel;
		private Logger _logger;

		public ContainerJobFactory(IKernel kernel, Logger logger)
		{
			_logger = logger;
			_kernel = kernel;
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
		{
			var type = bundle.JobDetail.JobType;

			_logger.Info("Job {0} was created", type.FullName);

			return _kernel.Get(type) as IJob;
		}
	}
}