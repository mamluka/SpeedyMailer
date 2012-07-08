using System;
using System.IO;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Ninject;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		public IKernel Kernel { get; set; }

		public DroneActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
			: base(kernel,taskManager,taskExecutor,scheduledTaskManager)
		{
			Kernel = kernel;
		}

		public Drone CreateDrone(string droneId)
		{
			var apiBaseUri = RandomHostname();
			EditSettings<IApiCallsSettings>(x => x.ApiBaseUri = apiBaseUri);
			new TopDrone(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper, Kernel.Get<IApiCallsSettings>());
			return new Drone
			       	{
			       		Hostname = apiBaseUri,
						Id = droneId,
			       	};
		}

		private string RandomHostname()
		{
			var randomizer = new Random();
			return "http://localhost:" + randomizer.Next(2000, 99999);
		}

		public override void EditSettings<T>(Action<T> action)
		{
			var name = SettingsFileName<T>();
			using (var writter = new StreamWriter(name))
			{
				var settings = Kernel.Get<T>();
				action(settings);
				writter.WriteLine(JsonConvert.SerializeObject(settings));
			}
		}

		private static string SettingsFileName<T>()
		{
			return typeof (T).Name.Substring(1).Replace("Settings", "") + ".settings";
		}
	}
}