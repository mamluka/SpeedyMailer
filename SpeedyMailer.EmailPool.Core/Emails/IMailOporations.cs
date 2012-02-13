using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.EmailPool.Core.Emails
{
    public interface IMailOporations
    {
        void Preform(PoolSideOporationBase poolSideOporation);
        void Preform(DroneSideOporationBase poolSideOporation);
    }
}