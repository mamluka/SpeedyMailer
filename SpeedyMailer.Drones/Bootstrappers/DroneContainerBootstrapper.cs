using Nancy.Bootstrapper;
using Ninject;
using Ninject.Modules;
using Quartz;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Drones.Bootstrappers
{
	public static class DroneContainerBootstrapper
	{
		public static IKernel Kernel { get; private set; }

		static DroneContainerBootstrapper()
		{
			Kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
					{
						typeof (DroneAssemblyMarker),
						typeof (CoreAssemblyMarker),
						typeof (ISchedulerFactory),
						typeof (IRestClient),
						typeof (IDocumentStore)
					}))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			var scheduler = Kernel.Get<IScheduler>();
			Kernel
				.Bind<INancyBootstrapper>()
				.ToProvider(new NancyBootstrapperProvider(
								kernel =>
								ContainerBootstrapper.Bootstrap(kernel).Analyze(
									x => x.AssembiesContaining(new[]
							            {
								            typeof (DroneAssemblyMarker),
								            typeof (CoreAssemblyMarker),
								            typeof (ISchedulerFactory),
								            typeof (IRestClient),
								            typeof (IDocumentStore)
							            }))
									.BindInterfaceToDefaultImplementation()
									.DefaultConfiguration()
									.NoDatabase()
									.Settings(x => x.UseJsonFiles())
									.Done(), scheduler)
				);

		}
	}
}