using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Hosting.Self;

namespace SpeedyMailer.MailDrone
{
    class Program
    {
        static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:45678/"));
            nancyHost.Start();



            Console.WriteLine("Mail drone now active - navigating to http://localhost:45678/. Press enter to stop");
            Console.ReadKey();

            nancyHost.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }
}
