using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.Core.Protocol
{
    public class FragmenRequest
    {
        public MailDrone MailDrone { get; set; }
        public PoolSideOporationBase PoolSideOporation { get; set; }
    }
}