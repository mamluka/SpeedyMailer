using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Ninject;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Drones.Bootstrappers;

namespace SpeedyMailer.Drones
{
    public class DroneHost
    {
        public static void Main(string[] args)
        {
        	var kernel = DroneContainerBootstrapper.Kernel;
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
		private readonly IApiCallsSettings _apiCallsSettings;

		public TopDrone(INancyBootstrapper nancyBootstrapper,IApiCallsSettings apiCallsSettings)
		{
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

	public interface ITransportSettings
	{
		string Host { get; set; }
	}
}