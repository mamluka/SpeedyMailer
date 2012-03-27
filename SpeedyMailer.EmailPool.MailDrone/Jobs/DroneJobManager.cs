using Quartz;
using Quartz.Impl;

namespace SpeedyMailer.EmailPool.MailDrone.Jobs
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
}