using System;
using Nancy.Hosting.Self;
using Ninject;
using SpeedyMailer.EmailPool.MailDrone.Bootstrappers;
using SpeedyMailer.EmailPool.MailDrone.Jobs;
using SpeedyMailer.EmailPool.MailDrone.Mail;

namespace SpeedyMailer.EmailPool.MailDrone
{
    public class MailDroneHost
    {
        private static NancyHost nancyHost;

        public static void Main(string[] args)
        {
            StartServer();
            StartJobManager();
            StopServer();
        }

        private static void StopServer()
        {
            nancyHost.Stop();
        }

        private static void StartJobManager()
        {
            var jobManager = new DroneJobManager();
            jobManager.StartRetrieveJob();
        }

        private static void StartServer()
        {
            nancyHost = new NancyHost(new Uri("http://localhost:45678/"));
            nancyHost.Start();
        }
    }
}
