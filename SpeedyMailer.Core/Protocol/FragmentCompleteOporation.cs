using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.Core.Protocol
{
    public class FragmentCompleteOporation:PoolSideOporationBase
    {
        public MailDrone CompletedBy { get; set; }
    }
}