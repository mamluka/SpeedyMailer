using Ninject;
using Quartz;
using Quartz.Spi;
using SpeedyMailer.EmailPool.MailDrone.Bootstrappers;

namespace SpeedyMailer.EmailPool.MailDrone.Jobs
{
    public class IoCJobFactory:IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var kernel = NinjectBootstrapper.Kernel;
            var type = bundle.JobDetail.JobType;
            return kernel.Get(type) as IJob;

        }
    }
}