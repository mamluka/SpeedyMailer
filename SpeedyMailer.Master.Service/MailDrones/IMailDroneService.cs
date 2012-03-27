using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.Master.Service.MailDrones
{
    public interface IMailDroneService
    {
        DroneStatus WakeUp(MailDrone mailDrone);
    }
}