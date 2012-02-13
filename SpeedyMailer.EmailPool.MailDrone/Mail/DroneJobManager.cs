using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
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

        public void Start()
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
        private readonly IMailOporations mailOporations;
        private readonly IMailSender mailSender;

        public RetrieveFragmentJob(IDroneCommunicationService droneCommunicationService, IMailOporations mailOporations, IMailSender mailSender)
        {
            this.droneCommunicationService = droneCommunicationService;
            this.mailOporations = mailOporations;
            this.mailSender = mailSender;
        }

        public void Execute(IJobExecutionContext context)
        {

            var fragment = droneCommunicationService.RetrieveFragment();

            foreach (var droneSideOporation in fragment.DroneSideOporations)
            {
                mailOporations.Preform(droneSideOporation);
            }

            mailSender.ProcessFragment(fragment.EmailFragment);

        }
    }
}
