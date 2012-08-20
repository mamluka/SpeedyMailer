using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Bootstrappers;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones
{
    public class DroneHost
    {
        public static void Main(string[] args)
        {
        	var kernel = DroneContainerBootstrapper.Kernel;

	        var initializeDroneSettingsCommand = kernel.Get<InitializeDroneSettingsCommand>();
	        initializeDroneSettingsCommand.RemoteConfigurationServiceBaseUrl = "http://localhost:12345";
			initializeDroneSettingsCommand.Execute();

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
		private readonly DroneSettings _droneSettings;

		public TopDrone(INancyBootstrapper nancyBootstrapper,DroneSettings droneSettings)
		{
			_droneSettings = droneSettings;
			_nancyBootstrapper = nancyBootstrapper;
		}

		public void Initialize()
		{
			_nancy = new NancyHost(new Uri(_droneSettings.BaseUrl), _nancyBootstrapper);
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