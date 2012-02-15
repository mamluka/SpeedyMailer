using System.Configuration;

namespace SpeedyMailer.Core.Helpers
{
    public class ControlRoomConfigurationManager : IConfigurationManager
    {

        public ControlRoomConfigurations ControlRoomConfigurations { get; private set; }

        public ControlRoomConfigurationManager()
        {
            ControlRoomConfigurations = new ControlRoomConfigurations()
                                            {
                                                DomainUrl = ByKey("DomainUrl")
                                            };
        }

        private string ByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

    }
}