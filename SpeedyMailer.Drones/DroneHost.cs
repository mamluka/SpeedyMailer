using System;
using System.Collections.Generic;
using System.Diagnostics;
using CommandLine;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Ninject;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Bootstrappers;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Drones
{
	public class DroneCommandOptions : CommandLineOptionsBase
	{
		[Option("s", "service-base-url", HelpText = "The base url of the service to register the drone with", Required = true)]
		public string ServiceBaseUrl { get; set; }

		[Option("b", "drone-base-url", HelpText = "The base url of the service to register the drone with", Required = false)]
		public string BaseUrl { get; set; }

		[Option("N", "no-tasks", DefaultValue = false, HelpText = "The base url of the service to register the drone with", Required = false)]
		public bool NoTasks { get; set; }

		[Option("P", "port", DefaultValue = 8080, HelpText = "The base url of the service to register the drone with", Required = false)]
		public int RedirectedToListeningPort { get; set; }
	}
	public class DroneHost
	{
		public static void Main(string[] args)
		{
			var kernel = DroneContainerBootstrapper.Kernel;

			var options = new DroneCommandOptions();

			if (CommandLineParser.Default.ParseArguments(args, options))
			{
				var initializeDroneSettingsCommand = kernel.Get<InitializeDroneSettingsCommand>();
				initializeDroneSettingsCommand.RemoteConfigurationServiceBaseUrl = options.ServiceBaseUrl;
				initializeDroneSettingsCommand.DroneBaseUrl = options.BaseUrl;
				initializeDroneSettingsCommand.Execute();

				var drone = kernel.Get<TopDrone>();

				drone.Initialize(options.RedirectedToListeningPort, options.NoTasks);
				drone.Start();

				Console.WriteLine("Starting drone...");
			}
		}
	}

	public class TopDrone
	{
		private NancyHost _nancy;
		private readonly INancyBootstrapper _nancyBootstrapper;
		private readonly DroneSettings _droneSettings;
		private readonly Framework _framework;
		private readonly Logger _logger;
		private readonly ApiCallsSettings _apiCallsSettings;

		public TopDrone(INancyBootstrapper nancyBootstrapper, Framework framework, DroneSettings droneSettings, ApiCallsSettings apiCallsSettings, Logger logger)
		{
			_apiCallsSettings = apiCallsSettings;
			_logger = logger;
			_framework = framework;
			_droneSettings = droneSettings;
			_nancyBootstrapper = nancyBootstrapper;
		}

		public void Initialize(int redirectedToListeningPort = 8080, bool noTasks = false)
		{
			if (!noTasks)
			{
				var tasks = new List<ScheduledTask>
				{
					new BroadcastDroneToServiceTask().DelayFor(TimeSpan.FromSeconds(30)),
					new FetchCreativeFragmentsTask().DelayFor(TimeSpan.FromMinutes(1)),
					new AnalyzePostfixLogsTask().DelayFor(TimeSpan.FromMinutes(5)),
					new FetchDeliveryClassificationHeuristicsTask().DelayFor(TimeSpan.FromSeconds(15)),
					new FetchIntervalRulesTask().DelayFor(TimeSpan.FromSeconds(30)),
					new SendDroneStateSnapshotTask().DelayFor(TimeSpan.FromSeconds(45)),
					new ResumePausedGroupsTask(),
				};

				_framework.StartTasks(tasks);
			}

			_nancy = new NancyHost(new Uri(string.Format("{0}:{1}", _droneSettings.BaseUrl, redirectedToListeningPort)), _nancyBootstrapper);
		}

		public void Start()
		{
			Trace.WriteLine("Drone started:" + _droneSettings.BaseUrl);

			_nancy.Start();

			_logger.Info("Drone started, master host is: {0}, drone host is: {1}", _apiCallsSettings.ApiBaseUri, new Uri(_droneSettings.BaseUrl).ToString());
		}

		public void Stop()
		{
			_logger.Info("Drone stopped");
			_nancy.Stop();
		}
	}
}