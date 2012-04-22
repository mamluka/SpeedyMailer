using System.Configuration;
using Castle.Core.Logging;

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


        public ControlRoomConfigurations ControlRoomConfigurations { get; private set; }


        private string ByKey(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}