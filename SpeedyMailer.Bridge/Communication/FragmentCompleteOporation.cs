using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Bridge.Communication
{
    public class FragmentCompleteOporation:PoolSideOporationBase
    {
        public MailDrone CompletedBy { get; set; }
    }
}