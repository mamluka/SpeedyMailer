using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPoolMaster.MailDrones
{
    public interface IMailDroneService
    {
        DroneStatus WakeUp(MailDrone mailDrone);
    }
}