using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Drones.Bootstrappers;

namespace SpeedyMailer.Drones
{
    public class DroneHost
    {
        public static void Main(string[] args)
        {
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
				                                    	{
				                                    		typeof (DroneAssemblyMarker),
				                                    		typeof (ISchedulerFactory)
				                                    	}))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();


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

		public TopDrone(INancyBootstrapper nancyBootstrapper,ApiCallsSettings apiCallsSettings)
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

	public class TransportSettings
	{
		public string Host { get; set; }
	}
}