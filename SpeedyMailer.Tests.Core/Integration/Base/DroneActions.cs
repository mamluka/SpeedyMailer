using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Master.Service;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		public IKernel Kernel { get; set; }

		private IList<int> RunningMongoPorts = new List<int>();

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
				var settings = AutoMapper.Mapper.DynamicMap<T, T>((kernel ?? Kernel).Get<T>());
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

			var randomizePost = RandomizePost();
			var storeUri = IntergrationHelpers.DefaultStoreUri(randomizePost);

			Trace.WriteLine(droneId + " has started mongo on: " + storeUri);

			MongoRunner.Start(randomizePost);
			RunningMongoPorts.Add(randomizePost);

			EditSettings<ApiCallsSettings>(x =>
											   {
												   x.ApiBaseUri = serviceBaseUrl;
											   }, droneKernel);
			EditSettings<DroneSettings>(x =>
											{
												x.BaseUrl = baseUrl;
												x.Identifier = droneId;
												x.StoreHostname = storeUri;
											}, droneKernel);

			EditSettings<EmailingSettings>(x =>
											   {
												   x.WritingEmailsToDiskPath = IntergrationHelpers.AssemblyDirectory;
											   }, droneKernel);

			droneKernel.Bind<IDocumentStore>().ToConstant(MockRepository.GenerateStub<IDocumentStore>());
			droneKernel.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InTransientScope();
			droneKernel.Rebind<INancyBootstrapper>().ToConstant(new DroneNancyNinjectBootstrapperForTesting() as INancyBootstrapper);

			var topDrone = droneKernel.Get<TopDrone>();

			return topDrone;
		}

		public void StopAllDroneStores()
		{
			var mongodb = new IntegrationMongoDbHelper();

			mongodb.ShutdownMongo(RunningMongoPorts);
		}

		public void Store<T>(T entity) where T : class
		{
			var manager = new RecordManager<T>(IntergrationHelpers.DefaultStoreUri());
			manager.BatchInsert(new List<T> { entity });
		}

		public void StoreCollection<T>(IEnumerable<T> collection, string collectionName = null) where T : class
		{
			var manager = new RecordManager<T>(IntergrationHelpers.DefaultStoreUri(), collectionName);
			manager.BatchInsert(collection);
		}


		public IList<T> FindAll<T>(string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(IntergrationHelpers.DefaultStoreUri(), collectionName);
			return manager.FindAll();
		}

		private int RandomizePost()
		{
			var random = new Random();
			return random.Next(1000, 10000);
		}

		public void WaitForDocumentToExist<T>(string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(IntergrationHelpers.DefaultStoreUri(), collectionName);

			var st = new Stopwatch();
			st.Start();
			
			while (st.ElapsedMilliseconds < 30 * 1000 && !manager.Exists())
			{
				Thread.Sleep(500);
			}

			st.Stop();
			manager.Exists().Should().BeTrue("Waiting for {0} to exist in the database", typeof(T).Name);
		}
	}
}