using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Quartz;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Service.Container;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class ServiceNancyNinjectBootstrapperForTesting : NinjectNancyBootstrapper
	{
		private readonly IDocumentStore _documentStore;

		public ServiceNancyNinjectBootstrapperForTesting(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		protected override void ConfigureApplicationContainer(IKernel existingContainer)
		{
			ContainerBootstrapper
				.Bootstrap(existingContainer)
				.Analyze(x => x.AssembiesContaining(new[]
				                                    	{
				                                    		typeof(CoreAssemblyMarker),
				                                    		typeof(ServiceAssemblyMarker),
				                                    		typeof(IRestClient)
				                                    	}))
				.BindInterfaceToDefaultImplementation()
				.Configure(x=> x.InTransientScope())
				.Storage<IDocumentStore>(x => x.Constant(_documentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			existingContainer.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InTransientScope();
		}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration.WithOverrides(
					   c => c.Serializers.Insert(0, typeof(JsonNetSerializer)));
			}
		}
	}

	public class DroneNancyNinjectBootstrapperForTesting : NinjectNancyBootstrapper
	{
		protected override void ConfigureApplicationContainer(IKernel existingContainer)
		{
			ContainerBootstrapper
				.Bootstrap(existingContainer)
				.Analyze(x => x.AssembiesContaining(new[]
				                                    	{
				                                    		typeof(CoreAssemblyMarker),
				                                    		typeof(DroneAssemblyMarker),
				                                    		typeof(IRestClient),
				                                    		typeof(IDocumentStore)
				                                    	}))
				.BindInterfaceToDefaultImplementation()
				.Configure(x => x.InTransientScope())
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			existingContainer.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InTransientScope();
		}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration.WithOverrides(
					   c => c.Serializers.Insert(0, typeof(JsonNetSerializer)));
			}
		}
	}
}