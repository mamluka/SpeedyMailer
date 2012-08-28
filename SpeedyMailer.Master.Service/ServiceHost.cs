using System;
using System.Diagnostics;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Container;
using Ninject;

namespace SpeedyMailer.Master.Service
{
    public class ServiceHost
    {
        public static void Main(string[] args)
        {
            var kernel = ServiceContainerBootstrapper.Kernel;

	        var framework = kernel.Get<Framework>();
			framework.EditStoreSettings<ServiceSettings>(x=>
				{
					x.BaseUrl = "http://10.0.0.1:9852";
				});

            var service = kernel.Get<TopService>();

            service.Start();
            Console.WriteLine("To stop press any key");
            Console.ReadKey();
            service.Stop();

	        var a = kernel.Get<IScheduler>();
			a.Shutdown();
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