using System;
using System.Dynamic;
using System.IO;
using ImpromptuInterface;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ninject;
using Rhino.Mocks;
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
			: base(kernel, taskManager, taskExecutor, scheduledTaskManager)
		{
			Kernel = kernel;
		}

		public Drone CreateDrone(string droneId)
		{
			var apiBaseUri = RandomHostname();
			EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = apiBaseUri);
			new TopDrone(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper, Kernel.Get<ApiCallsSettings>());
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
			var settingsFolder = Directory.CreateDirectory("settings");

			using (var writter = new StreamWriter(Path.Combine(settingsFolder.FullName, name)))
			{
				dynamic settings = new T();
				action(settings);
				writter.WriteLine(JsonConvert.SerializeObject(settings,
				                                              Formatting.Indented,
				                                              new JsonSerializerSettings
				                                              	{
				                                              		NullValueHandling = NullValueHandling.Ignore
				                                              	}));
			}
		}

		private static string SettingsFileName<T>()
		{
			return typeof(T).Name.Replace("Settings", "") + ".settings";
		}
	}
}