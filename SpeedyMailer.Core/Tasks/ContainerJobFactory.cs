using System.Diagnostics;
using Ninject;
using Quartz;
using Quartz.Spi;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;

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
			Trace.WriteLine("Now resolving job name: " + type.FullName);

			var a = _kernel.Get<NinjectIdentitySettings>();
			Trace.WriteLine("Creating kernel of job factory is: " + a.KernelName);


			return _kernel.Get(type) as IJob;
		}
	}
}