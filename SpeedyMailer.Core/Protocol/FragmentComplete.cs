using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.Core.Protocol
{
    public class FragmentComplete:PoolSideOporationBase
    {
        public MailDrone CompletedBy { get; set; }
    }
}