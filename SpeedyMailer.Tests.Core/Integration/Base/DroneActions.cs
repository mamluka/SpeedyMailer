using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MongoDB.Driver;
using MongoDB.Runner;
using Mongol;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Quartz;
using Raven.Client;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Master.Service;

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

		public override void EditSettings<T>(Action<T> action, IKernel kernel = null)
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

			ContainerBootstrapper.ReloadJsonSetting<T>(kernel ?? Kernel, settingsFolderName);
		}

		private static string SettingsFileName<T>()
		{
			return typeof(T).Name.Replace("Settings", "") + ".settings";
		}

		public TopDrone CreateDrone(string droneId, string baseUrl, string serviceBaseUrl)
		{
			var droneKernel = ContainerBootstrapper
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

			droneKernel.Bind<IDocumentStore>().ToConstant(MockRepository.GenerateStub<IDocumentStore>());
			droneKernel.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InTransientScope();

			EditSettings<ApiCallsSettings>(x =>
											   {
												   x.ApiBaseUri = serviceBaseUrl;
											   }, droneKernel);
			EditSettings<DroneSettings>(x =>
											{
												x.BaseUrl = baseUrl;
												x.Identifier = droneId;
											}, droneKernel);

			EditSettings<EmailingSettings>(x => x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory, droneKernel);

			droneKernel.Rebind<INancyBootstrapper>().ToConstant(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper);

			var topDrone = droneKernel.Get<TopDrone>();

			return topDrone;
		}

		public void Store<T>(T entity) where T : class
		{
			var manager = new RecordManager<T>();
			manager.BatchInsert(new List<T> { entity });
		}
		
		public void StorCollection<T>(IEnumerable<T> collection) where T : class
		{
			var manager = new RecordManager<T>();
			manager.BatchInsert(collection);
		}

		public void StartMongo()
		{
			MongoRunner.Start();
		}

		public void ShutdownMongo()
		{
			MongoRunner.Shutdown();
		}
	}
}