using System;
using Nancy.Hosting.Self;

namespace SpeedyMailer.Master.Web.UI.Drone
{
    public class MailDronInitializer
    {
        public MailDronInitializer()
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:45678/"));
        }

        public void Initialize()
        {
        }
    }
}