using SpeedyMailer.Bridge.Communication;

namespace SpeedyMailer.Master.Service.Emails
{
    public interface IPoolMailOporations
    {
        void Preform(PoolSideOporationBase poolSideOporation);
    }
}