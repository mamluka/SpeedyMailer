using System;
using System.Linq;
using NUnit.Framework;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Raven.Client;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
	[TestFixture]
	public class SendCreativeCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenAnEmailId_SendApiRequestToMasterService()
		{
			ServiceHost.Main(new IntegrationNancyNinjectBootstrapper(DocumentStore));

			const string creativeId = "email/1";

			Master.ExecuteCommand<SendCreativeCommand>(x=>
			                                           	{
			                                           		x.CreativeId = creativeId;
			                                           	});
		}
	}

	public class SendCreativeCommand:Command
	{
		public string CreativeId { get; set; }

		public override void Execute()
		{
			
		}
		
	}

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
                                                            typeof (CoreAssemblyMarker),
                                                            typeof(ServiceAssemblyMarker),
															typeof(IRestClient)
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.Storage<IDocumentStore>(x => x.Constant(_documentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();
		}
	}
}
