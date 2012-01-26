using System;
using Nancy.Hosting.Self;
using SpeedyMailer.EmailPool.MailDrone.Bootstrappers;

namespace SpeedyMailer.EmailPool.MailDrone
{
    class MailDrone
    {
        static void Main(string[] args)
        {
            var nancyHost = new NancyHost(new Uri("http://localhost:45678/"));
            nancyHost.Start();

            NinjectBootstrapper.Bootstrap();

            Console.WriteLine("Mail drone now active - navigating to http://localhost:45678/. Press enter to stop");
            Console.ReadKey();

            nancyHost.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }
}
