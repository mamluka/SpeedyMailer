using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPool.Master.MailDrones
{
    public class FragmenRequest
    {
        public MailDrone MailDrone { get; set; }
        public PoolSideOporationBase PoolSideOporation { get; set; }
    }
}