using System;
using Nancy.Hosting.Self;
using SpeedyMailer.Drone.Tasks;

namespace SpeedyMailer.Drone
{
    public class MailDroneHost
    {
        private static NancyHost nancyHost;

        public static void Main(string[] args)
        {
//            var drone = new MailDronInitializer();
//            drone.Initialize();
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