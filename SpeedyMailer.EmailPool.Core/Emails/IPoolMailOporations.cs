using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.EmailPool.Core.Emails
{
    public interface IPoolMailOporations
    {
        void Preform(PoolSideOporationBase poolSideOporation);
    }
}