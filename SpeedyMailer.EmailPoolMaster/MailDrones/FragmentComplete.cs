using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.EmailPool.Master.MailDrones
{
    public class FragmentComplete:PoolSideOporationBase
    {
        public MailDrone CompletedBy { get; set; }
    }
}