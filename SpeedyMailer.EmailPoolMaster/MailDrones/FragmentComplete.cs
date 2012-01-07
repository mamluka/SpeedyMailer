using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPoolMaster.MailDrones
{
    public class FragmentComplete:PoolSideOporationBase
    {
        public MailDrone CompletedBy { get; set; }
    }
}