using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Hosting.Self;

namespace SpeedyMailer.EmailPool.MailDrone.Drone
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
