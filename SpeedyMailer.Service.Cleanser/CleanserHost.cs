using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Nancy.Bootstrapper;
using Ninject;
using Quartz;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Console;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Service.Cleanser
{
	class CleanserHost
	{
		static void Main(string[] args)
		{
			var options = new MasterHostedServicesCommandOptions();

			if (CommandLineParser.Default.ParseArguments(args, options))
			{
				var kernel = CleanserContainerBootstrapper.Kernel;

				var initializeServiceSettingsCommand = kernel.Get<InitializeMasterHostedServicesSettingsCommand>();
				initializeServiceSettingsCommand.BaseUrl = options.BaseUrl;
				initializeServiceSettingsCommand.Execute();

				var service = kernel.Get<TopCleanser>();

				service.Start();
				Console.WriteLine("To stop press any key");
				Console.ReadKey();
				service.Stop();

				var scheduler = kernel.Get<IScheduler>();
				scheduler.Shutdown();
			}
		}

	public class TopCleanser
	{
		public void Start()
		{
			
		}

		public void Stop()
		{
			
		}
	}

	public class CleanserContainerBootstrapper
	{
		public static IKernel Kernel { get; private set; }

        static CleanserContainerBootstrapper()
        {
            Kernel = ApplyBindLogic(ContainerBootstrapper.Bootstrap());

			var scheduler = Kernel.Get<IScheduler>();
			Kernel.Bind<INancyBootstrapper>().ToProvider(new NancyBootstrapperProvider(
															kernel =>
															ApplyBindLogic(ContainerBootstrapper.Bootstrap(kernel)), scheduler)
				);
        }

        public static IKernel ApplyBindLogic(AssemblyGatherer assemblyGatherer)
        {
            return assemblyGatherer.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (CoreAssemblyMarker),
                                                            typeof (IRestClient),
                                                            typeof (ISchedulerFactory)
                                                        }))
                .BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
                .NoDatabase()
                .Settings(x => x.UseDocumentDatabase())
                .Done();
        }
    }
	}
}
