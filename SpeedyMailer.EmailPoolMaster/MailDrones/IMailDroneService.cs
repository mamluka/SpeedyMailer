using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPool.Master.MailDrones
{
    public interface IMailDroneService
    {
        DroneStatus WakeUp(MailDrone mailDrone);
        DroneStatus PutAsleep(MailDrone mailDrone);
    }
}