namespace SpeedyMailer.Core.MailDrones
{
    public class MailDrone
    {
        public string WakeUpUri { get; set; }

        public DroneStatus Status { get; set; }

        public string BaseUri { get; set; }
    }
}