using System;
using System.IO;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Ninject;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Master.Service.Container;

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
			new TopDrone(new DroneNancyNinjectBootstrapperForTesting(),Kernel.Get<DroneSettings>());
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
			var settingsGuid = Guid.NewGuid();
			var name = SettingsFileName<T>();
			var settingsFolder = Directory.CreateDirectory("settings_" + settingsGuid);

			var settingsFolderName = settingsFolder.FullName;
			using (var writter = new StreamWriter(Path.Combine(settingsFolderName, name)))
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

			ContainerBootstrapper.ReloadJsonSetting<T>(Kernel,settingsFolderName);
		}

		private static string SettingsFileName<T>()
		{
			return typeof(T).Name.Replace("Settings", "") + ".settings";
		}
	}
}