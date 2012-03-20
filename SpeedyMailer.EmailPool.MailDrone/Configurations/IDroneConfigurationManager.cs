using System.Configuration;

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

        public DroneConfigurationManager()
        {
            BasePoolUrl = GetValueFromConfiguration("BasePoolUrl");

            PoolOporationsUrls = new PoolOporationsUrls()
                                     {
                                         PopFragmentUrl = GetValueFromConfiguration("PopFragmentUrl")
                                     };
        }

        private string GetValueFromConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}