﻿using System;
using System.Diagnostics;
using CommandLine;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Console;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Container;
using Ninject;

namespace SpeedyMailer.Master.Service
{
	public class ServiceHost
	{
		public static void Main(string[] args)
		{
			var options = new MasterHostedServicesCommandOptions();

			if (CommandLineParser.Default.ParseArguments(args, options))
			{
				var kernel = ServiceContainerBootstrapper.Kernel;

				var initializeServiceSettingsCommand = kernel.Get<InitializeMasterHostedServicesSettingsCommand>();
				initializeServiceSettingsCommand.BaseUrl = options.BaseUrl;
				initializeServiceSettingsCommand.Execute();

				var addIndexesToRavenCommand = kernel.Get<AddIndexesToRavenCommand>();
				addIndexesToRavenCommand.Execute();

				var service = kernel.Get<TopService>();

				service.Start();
				Console.WriteLine("To stop press any key");
				Console.ReadKey();
				service.Stop();

				var scheduler = kernel.Get<IScheduler>();
				scheduler.Shutdown();
			}
		}
	}

	public class TopService
	{
		private NancyHost _nancyHost;
		private readonly Logger _logger;
		private readonly IScheduler _scheduler;
		private readonly INancyBootstrapper _ninjectNancyBootstrapper;
		private readonly ServiceSettings _serviceSettings;

		public TopService(INancyBootstrapper ninjectNancyBootstrapper, ServiceSettings serviceSettings, Logger logger, IScheduler scheduler)
		{
			_serviceSettings = serviceSettings;
			_ninjectNancyBootstrapper = ninjectNancyBootstrapper;
			_scheduler = scheduler;
			_logger = logger;

			logger.Info("Service was created with base url {0}", serviceSettings.BaseUrl);
			Trace.WriteLine("Service was created with base url: "+serviceSettings.BaseUrl);
		}

		public void Start()
		{
			_nancyHost = new NancyHost(new Uri(_serviceSettings.BaseUrl), _ninjectNancyBootstrapper);
			_nancyHost.Start();
		}

		public void Stop()
		{
			_nancyHost.Stop();
			_scheduler.Shutdown();

			_logger.Info("Stopping service");
			Console.WriteLine("Stopped. Good bye!");
		}
	}
}