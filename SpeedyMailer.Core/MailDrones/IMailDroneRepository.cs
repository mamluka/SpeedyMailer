using System.Collections.Generic;

namespace SpeedyMailer.Core.MailDrones
{
    public interface IMailDroneRepository
    {
        List<MailDrone> CurrentlySleepingDrones();
        void Update(MailDrone mailDrone);
    }
}