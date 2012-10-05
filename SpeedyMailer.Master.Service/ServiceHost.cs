using System;
using System.Diagnostics;
using CommandLine;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Container;
using Ninject;

namespace SpeedyMailer.Master.Service
{
    public class ServiceCommandOptions : CommandLineOptionsBase
    {
        [Option("b", "base-url", DefaultValue = @"http://localhost:9852", HelpText = "The base url of the service to register the drone with")]
        public string BaseUrl { get; set; }
    }

    public class ServiceHost
    {
        public static void Main(string[] args)
        {
            var options = new ServiceCommandOptions();

            if (CommandLineParser.Default.ParseArguments(args, options))
            {
                var kernel = ServiceContainerBootstrapper.Kernel;

                var initializeServiceSettingsCommand = kernel.Get<InitializeServiceSettingsCommand>();
                initializeServiceSettingsCommand.BaseUrl = options.BaseUrl;
                initializeServiceSettingsCommand.Execute();

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
        private readonly NancyHost _nancyHost;

        public TopService(INancyBootstrapper ninjectNancyBootstrapper,ServiceSettings serviceSettings)
        {
            Console.WriteLine(serviceSettings.BaseUrl);

            _nancyHost = new NancyHost(new Uri(serviceSettings.BaseUrl), ninjectNancyBootstrapper);
        }

        public void Start()
        {
            _nancyHost.Start();
        }

        public void Stop()
        {
            _nancyHost.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }
}