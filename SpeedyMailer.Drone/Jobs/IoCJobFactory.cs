using System;
using Ninject;
using Quartz;
using Quartz.Spi;
using SpeedyMailer.Master.Web.UI.Bootstrappers;

namespace SpeedyMailer.Master.Web.UI.Jobs
{
    public class IoCJobFactory : IJobFactory
    {
        #region IJobFactory Members

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IKernel kernel = NinjectBootstrapper.Kernel;
            Type type = bundle.JobDetail.JobType;
            return kernel.Get(type) as IJob;
        }

        #endregion
    }
}