using System;
using System.IO;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Ninject;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Master.Service;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		private TopDrone _topDrone;
		public IKernel Kernel { get; set; }

		public DroneActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
			: base(kernel, taskManager, taskExecutor, scheduledTaskManager)
		{
			Kernel = kernel;
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

			ContainerBootstrapper.ReloadJsonSetting<T>(Kernel, settingsFolderName);
		}

		private static string SettingsFileName<T>()
		{
			return typeof(T).Name.Replace("Settings", "") + ".settings";
		}

		public void Initialize(string baseUrl, string serviceBaseUrl)
		{
			EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = serviceBaseUrl);
			EditSettings<DroneSettings>(x => x.BaseUrl = baseUrl);
			Kernel.Rebind<INancyBootstrapper>().ToConstant(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper);

			_topDrone = Kernel.Get<TopDrone>();

			_topDrone.Initialize();
		}

		public void Start()
		{
			_topDrone.Start();
		}
	}
}