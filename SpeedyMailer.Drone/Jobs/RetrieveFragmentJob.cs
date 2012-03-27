using Quartz;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Master.Web.UI.Communication;
using SpeedyMailer.Master.Web.UI.Mail;

namespace SpeedyMailer.Master.Web.UI.Jobs
{
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
