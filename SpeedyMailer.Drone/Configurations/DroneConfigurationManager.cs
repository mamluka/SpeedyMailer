using System.Configuration;

namespace SpeedyMailer.Master.Web.UI.Configurations
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

        #region IDroneConfigurationManager Members

        public string BasePoolUrl { get; set; }
        public PoolOporationsUrls PoolOporationsUrls { get; set; }

        #endregion

        private string GetValueFromConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}