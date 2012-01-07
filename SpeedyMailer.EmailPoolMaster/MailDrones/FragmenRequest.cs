using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPoolMaster.MailDrones
{
    public class FragmenRequest
    {
        public MailDrone MailDrone { get; set; }
        public PoolSideOporationBase PoolSideOporation { get; set; }
    }
}