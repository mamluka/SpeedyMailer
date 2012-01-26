using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using SpeedyMailer.EmailPool.MailDrone.Bootstrappers;

namespace SpeedyMailer.EmailPool.MailDrone.Mail
{
    public class MailingProcess
    {
        public void Start()
        {
            var scheduleFactory = new StdSchedulerFactory();
            var scheduler = scheduleFactory.GetScheduler();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("MailTrigger")
               .StartNow()
               .Build();

            var job = JobBuilder.Create<SimpleRetrieveMailJob>()
               .WithIdentity("RetrieveJob")
               .Build();

            scheduler.ScheduleJob(job, trigger);

        }


    }
    [DisallowConcurrentExecution]
    public class SimpleRetrieveMailJob:IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var kernel = NinjectBootstrapper.Kernel;
        }
    }
}
