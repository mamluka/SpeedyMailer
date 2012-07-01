using System;
using Nancy.Bootstrapper;
using Ninject;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Drones;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		public IKernel Kernel { get; set; }
		public DroneActions(IKernel kernel)
			: base(kernel)
		{
			Kernel = kernel;
		}

		public Drone CreateDrone(string droneId)
		{
			var apiBaseUri = RandomHostname();
			EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = apiBaseUri);
			var drone = new TopDrone(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper, Kernel.Get<IApiCallsSettings>());
			return new Drone
			       	{
			       		Hostname = apiBaseUri,
						Identifier = droneId,
			       	};
		}

		private string RandomHostname()
		{
			var randomizer = new Random();
			return "http://localhost:" + randomizer.Next(2000, 99999);
		}

		public override void EditSettings<T>(Action<T> expression)
		{

		}
	}
}