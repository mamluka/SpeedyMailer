using Quartz;
using Quartz.Impl;

namespace SpeedyMailer.Drone.Tasks
{
    public class DroneJobManager
    {
        private readonly IScheduler _scheduler;

        public DroneJobManager()
        {
            var scheduleFactory = new StdSchedulerFactory();
            _scheduler = scheduleFactory.GetScheduler();
           // var IoCjobFactory = new ContainerJobFactory();
           // _scheduler.JobFactory = IoCjobFactory;
        }

        public void StartRetrieveJob()
        {
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("MailTrigger")
                .StartNow()
                .Build();

            IJobDetail job = JobBuilder.Create<RetrieveFragmentJob>()
                .WithIdentity("RetrieveJob")
                .Build();


            _scheduler.ScheduleJob(job, trigger);

            _scheduler.Start();
        }
    }
}