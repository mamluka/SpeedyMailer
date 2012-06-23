using Nancy.Bootstrappers.Ninject;
using Ninject;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Service;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationNancyNinjectBootstrapper : NinjectNancyBootstrapper
	{
		private readonly IDocumentStore _documentStore;

		public IntegrationNancyNinjectBootstrapper(IDocumentStore documentStore)
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
		}
	}
}