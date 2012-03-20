using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.Core.Emails;
using SpeedyMailer.EmailPool.MailDrone.Bootstrappers;
using Ninject;
using SpeedyMailer.EmailPool.MailDrone.Communication;

namespace SpeedyMailer.EmailPool.MailDrone.Mail
{
    public class DroneJobManager
    {
        private readonly IScheduler scheduler;

        public DroneJobManager()
        {
            var scheduleFactory = new StdSchedulerFactory();
            scheduler = scheduleFactory.GetScheduler();
            var IoCjobFactory = new IoCJobFactory();
           scheduler.JobFactory = IoCjobFactory;
        }

        public void StartRetrieveJob()
        {
            var trigger = TriggerBuilder.Create()
               .WithIdentity("MailTrigger")
               .StartNow()
               .Build();

            var job = JobBuilder.Create<RetrieveFragmentJob>()
               .WithIdentity("RetrieveJob")
               .Build();

            
            scheduler.ScheduleJob(job, trigger);

            scheduler.Start();

        }


    }

    public class IoCJobFactory:IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var kernel = NinjectBootstrapper.Kernel;
            var type = bundle.JobDetail.JobType;
            return kernel.Get(type) as IJob;

        }
    }
    [DisallowConcurrentExecution]
    public class RetrieveFragmentJob:IJob
    {
        private readonly IDroneCommunicationService droneCommunicationService;
        private readonly IDroneMailOporations mailOporations;
        private readonly IMailSender mailSender;

        public RetrieveFragmentJob(IDroneCommunicationService droneCommunicationService, IDroneMailOporations mailOporations, IMailSender mailSender)
        {
            this.droneCommunicationService = droneCommunicationService;
            this.mailOporations = mailOporations;
            this.mailSender = mailSender;
        }

        public void Execute(IJobExecutionContext context)
        {
            var stopJob = false;

            var fragment = droneCommunicationService.RetrieveFragment();

            mailOporations.StopCurrentJob = () => stopJob = true;

            if (fragment.DroneSideOporations != null)
            {
                foreach (var droneSideOporation in fragment.DroneSideOporations)
                {
                    mailOporations.Preform(droneSideOporation);
                }
            }

            mailSender.ProcessFragment(fragment.EmailFragment);
            
            if (!stopJob)
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("MailTrigger")
                    .StartNow()
                    .Build();

                context.Scheduler.RescheduleJob(new TriggerKey("MailTrigger"), trigger);
            }

           
        }
    }
}
