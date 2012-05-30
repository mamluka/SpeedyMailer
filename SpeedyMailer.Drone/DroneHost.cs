using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drone.Bootstrappers;
using Ninject;

namespace SpeedyMailer.Drone
{
    public class DroneHost
    {
        public static void Main(string[] args)
        {
        	var kernel = DroneContainerBootstrapper.Kernel;
        	var drone = kernel.Get<Drone>();
			drone.Start();
			Console.WriteLine("Starting drone...");
        	Console.ReadKey();
        }
    }

	public class Drone
	{
		private NancyHost _nancy;
		private readonly ITransportSettings _transportSettings;
		private readonly INancyBootstrapper _nancyBootstrapper;
		private readonly Framework _framework;

		public Drone(INancyBootstrapper nancyBootstrapper,ITransportSettings transportSettings,Framework framework)
		{
			_framework = framework;
			_nancyBootstrapper = nancyBootstrapper;
			_transportSettings = transportSettings;
		}

		public void Initialize()
		{
			_nancy = new NancyHost(_nancyBootstrapper, new Uri(_transportSettings.Host));
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

	public interface ITransportSettings
	{
		string Host { get; set; }
	}
}