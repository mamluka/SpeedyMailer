using System.Configuration;

namespace SpeedyMailer.Drone.Configurations
{
    internal class DroneConfigurationManager : IDroneConfigurationManager
    {
        public DroneConfigurationManager()
        {
            BasePoolUrl = GetValueFromConfiguration("BasePoolUrl");

            PoolOporationsUrls = new PoolOporationsUrls
                                     {
                                         PopFragmentUrl = GetValueFromConfiguration("PopFragmentUrl")
                                     };
        }

        public string BasePoolUrl { get; set; }
        public PoolOporationsUrls PoolOporationsUrls { get; set; }

        private string GetValueFromConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}