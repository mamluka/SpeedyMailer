using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Bridge.Communication
{
    public class FragmenRequest
    {
        public MailDrone MailDrone { get; set; }
        public PoolSideOporationBase PoolSideOporation { get; set; }
    }
}