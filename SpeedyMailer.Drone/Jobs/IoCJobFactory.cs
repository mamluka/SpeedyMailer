using Ninject;
using Quartz;
using Quartz.Spi;
using SpeedyMailer.Master.Web.UI.Bootstrappers;

namespace SpeedyMailer.Master.Web.UI.Jobs
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