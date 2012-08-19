using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones
{
    public class DroneHost
    {
        public static void Main(string[] args)
        {
        	var kernel = ContainerBootstrapper.Kernel;


				

        	var drone = kernel.Get<TopDrone>();
			drone.Start();
			Console.WriteLine("Starting drone...");
        	Console.ReadKey();
        }
    }

	public class TopDrone
	{
		private NancyHost _nancy;
		private readonly INancyBootstrapper _nancyBootstrapper;
		private readonly ApiCallsSettings _apiCallsSettings;
		private DroneSettings _droneSettings;

		public TopDrone(INancyBootstrapper nancyBootstrapper,DroneSettings droneSettings,ApiCallsSettings apiCallsSettings)
		{
			_droneSettings = droneSettings;
			_apiCallsSettings = apiCallsSettings;
			_nancyBootstrapper = nancyBootstrapper;
		}

		public void Initialize()
		{
			_nancy = new NancyHost(new Uri(_apiCallsSettings.ApiBaseUri), _nancyBootstrapper);
		}

		public void Start()
		{
			_nancy.Start();
		}

		public void Stop()
		{
			_nancy.Stop();
		}
	}

	public class TransportSettings
	{
		public string Host { get; set; }
	}
}