using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Quartz;
using Raven.Client;
using Raven.Client.Document;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Drones;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public static class ContainersConfigurationsForTesting
	{
		public static readonly IDocumentStore MockedDocumentStore = MockRepository.GenerateStub<IDocumentStore>();
		
			public static IKernel Service()
		{
			return ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (CoreAssemblyMarker),
                                                            typeof (WebCoreAssemblyMarker),
                                                            typeof (ServiceAssemblyMarker),
                                                            typeof (IRestClient),
                                                            typeof (ISchedulerFactory)
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.Configure(x => x.InTransientScope())
				.Storage<IDocumentStore>(x => x.Constant(MockedDocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();
		}

		public static IKernel Drone()
		{
			return ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (DroneAssemblyMarker),
                                                            typeof (ISchedulerFactory),
                                                            typeof (CoreAssemblyMarker),
                                                            typeof(IRestClient)
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.Configure(x => x.InTransientScope())
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();
		}
	}
}
