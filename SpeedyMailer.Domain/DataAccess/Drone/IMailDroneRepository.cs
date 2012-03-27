using System.Collections.Generic;
using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Domain.DataAccess.Drone
{
    public interface IMailDroneRepository
    {
        List<MailDrone> CurrentlySleepingDrones();
        void Update(MailDrone mailDrone);
    }
}