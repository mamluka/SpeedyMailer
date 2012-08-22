using Nancy.Bootstrapper;
using Ninject;
using Ninject.Modules;
using Quartz;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Service.Container;

namespace SpeedyMailer.Drones.Container
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
		}
	}

	public class NancyModule : NinjectModule
	{
		public override void Load()
		{
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
						            .Done())
				);
		}
	}
}