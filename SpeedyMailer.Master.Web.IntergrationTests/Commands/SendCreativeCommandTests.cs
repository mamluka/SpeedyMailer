using System;
using System.Linq;
using NUnit.Framework;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Rhino.Mocks;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
	[TestFixture]
	public class SendCreativeCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenAnEmailId_SendApiRequestToMasterService()
		{
			ServiceHost.Main(new IntegrationNancyNinjectBootstrapper());

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
		private readonly Action<IKernel> _kernelAction;

		public IntegrationNancyNinjectBootstrapper(Action<IKernel> kernelAction)
		{
			_kernelAction = kernelAction;
		}

		public IntegrationNancyNinjectBootstrapper()
		{
			
		}
		protected override void ConfigureApplicationContainer(IKernel existingContainer)
		{
			_kernelAction.Invoke(existingContainer);
		}
	}
}
