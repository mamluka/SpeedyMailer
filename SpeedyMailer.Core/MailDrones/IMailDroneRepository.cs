using System.Collections.Generic;

namespace SpeedyMailer.Core.MailDrones
{
    public interface IMailDroneRepository
    {
        List<MailDrone> SleepingDrones();
        void Update(MailDrone mailDrone);
    }
}