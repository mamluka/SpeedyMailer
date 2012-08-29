using System;
using System.Collections.Generic;
using CommandLine;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Bootstrappers;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Drones
{
	public class DroneCommandOptions:CommandLineOptionsBase
	{
		[Option("s","service-base-url", DefaultValue  = @"http://localhost:2589",HelpText = "The base url of the service to register the drone with")]
		public string ServiceBaseUrl { get; set; }
	}
    public class DroneHost
    {
        public static void Main(string[] args)
        {
	        var options = new DroneCommandOptions();

			if (CommandLineParser.Default.ParseArguments(args, options))
			{
				var kernel = DroneContainerBootstrapper.Kernel;

				var initializeDroneSettingsCommand = kernel.Get<InitializeDroneSettingsCommand>();
				initializeDroneSettingsCommand.RemoteConfigurationServiceBaseUrl = "http://localhost:12345";
				initializeDroneSettingsCommand.Execute();

				var drone = kernel.Get<TopDrone>();

				drone.Start();

				Console.WriteLine("Starting drone...");
				
			}

			Console.ReadKey();
        }
    }

	public class TopDrone
	{
		private NancyHost _nancy;
		private readonly INancyBootstrapper _nancyBootstrapper;
		private readonly DroneSettings _droneSettings;
		private readonly Framework _framework;

		public TopDrone(INancyBootstrapper nancyBootstrapper,Framework framework,DroneSettings droneSettings)
		{
			_framework = framework;
			_droneSettings = droneSettings;
			_nancyBootstrapper = nancyBootstrapper;
		}

		public void Initialize()
		{
			var tasks = new List<ScheduledTask>
				{
					new BroadcastDroneToServiceTask(),
					new FetchCreativeFragmentsTask()
				};

			_framework.StartTasks(tasks);

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