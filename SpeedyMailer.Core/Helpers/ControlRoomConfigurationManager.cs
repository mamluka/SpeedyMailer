using System.Configuration;

namespace SpeedyMailer.Core.Helpers
{
    public class ControlRoomConfigurationManager : IConfigurationManager
    {
        public ControlRoomConfigurationManager()
        {
            ControlRoomConfigurations = new ControlRoomConfigurations
                                            {
                                                DomainUrl = ByKey("DomainUrl")
                                            };
        }

        #region IConfigurationManager Members

        public ControlRoomConfigurations ControlRoomConfigurations { get; private set; }

        #endregion

        private string ByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}