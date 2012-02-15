namespace SpeedyMailer.EmailPool.MailDrone.Configurations
{
    public interface IDroneConfigurationManager
    {
        string BasePoolUrl { get; set; }
        PoolOporationsUrls PoolOporationsUrls { get; set; }

    }

    class DroneConfigurationManager : IDroneConfigurationManager
    {
        public string BasePoolUrl { get; set; }
        public PoolOporationsUrls PoolOporationsUrls { get; set; }
    }
}