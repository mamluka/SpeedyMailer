using SpeedyMailer.Bridge.Communication;

namespace SpeedyMailer.Master.Service.Core.Emails
{
    public interface IPoolMailOporations
    {
        void Preform(PoolSideOporationBase poolSideOporation);
    }
}