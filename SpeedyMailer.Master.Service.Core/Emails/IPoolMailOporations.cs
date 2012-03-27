using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.Master.Service.Core.Emails
{
    public interface IPoolMailOporations
    {
        void Preform(PoolSideOporationBase poolSideOporation);
    }
}