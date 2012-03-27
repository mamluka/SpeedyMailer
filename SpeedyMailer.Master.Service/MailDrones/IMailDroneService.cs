using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Master.Service.MailDrones
{
    public interface IMailDroneService
    {
        DroneStatus WakeUp(MailDrone mailDrone);
    }
}