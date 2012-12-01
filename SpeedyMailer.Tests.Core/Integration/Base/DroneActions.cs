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
using Nancy.Hosting.Self;
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
using SpeedyMailer.Drones.Tasks;
using SpeedyMailer.Master.Service;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		public IKernel Kernel { get; set; }

		private readonly IList<int> _runningMongoPorts = new List<int>();
		private readonly IDictionary<string, string> _runningMongoUrls = new Dictionary<string, string>();
		private NancyHost _nancy;

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
			_runningMongoPorts.Add(randomizePost);
			_runningMongoUrls.Add(droneId, storeUri);

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
			droneKernel.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InSingletonScope();

			droneKernel.Rebind<INancyBootstrapper>().ToConstant(new DroneNancyNinjectBootstrapperForTesting(Kernel, droneKernel.Get<IScheduler>()) as INancyBootstrapper);

			var topDrone = droneKernel.Get<TopDrone>();

			return topDrone;
		}

		public void StopAllDroneStores()
		{
			var mongodb = new IntegrationMongoDbHelper(Kernel.Get<DroneSettings>().StoreHostname);

			mongodb.ShutdownMongo(_runningMongoPorts);
		}

		public void Store<T>(T entity) where T : class
		{
			var connectionString = Kernel.Get<DroneSettings>().StoreHostname;

			var manager = new RecordManager<T>(connectionString);
			manager.BatchInsert(new List<T> { entity });
		}

		public void StoreCollection<T>(IEnumerable<T> collection, string collectionName = null) where T : class
		{
			var connectionString = Kernel.Get<DroneSettings>().StoreHostname;

			var manager = new RecordManager<T>(connectionString, collectionName);
			manager.BatchInsert(collection);
		}

		public void StoreCollectionForDrone<T>(IEnumerable<T> collection, string droneId, string collectionName = null) where T : class
		{
			var connectionString = _runningMongoUrls.ContainsKey(droneId) ? _runningMongoUrls[droneId] : Kernel.Get<DroneSettings>().StoreHostname;

			var manager = new RecordManager<T>(connectionString, collectionName);
			manager.BatchInsert(collection);
		}

		public IList<T> FindAll<T>(string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(Kernel.Get<DroneSettings>().StoreHostname, collectionName);
			return manager.FindAll();
		}

		private int RandomizePost()
		{
			var random = new Random();
			return random.Next(1000, 10000);
		}

		public void WaitForDocumentToExist<T>(int count = 1, string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(Kernel.Get<DroneSettings>().StoreHostname, collectionName);

			var st = new Stopwatch();
			st.Start();

			while (st.ElapsedMilliseconds < 30 * 1000 && !manager.Exists(count))
			{
				Thread.Sleep(500);
			}

			st.Stop();
			manager.Exists().Should().BeTrue("Waiting for {0} to exist in the database {1} times but found {2}", typeof(T).Name, count, manager.Collection.Count());
		}

		public T FindSingle<T>(string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(Kernel.Get<DroneSettings>().StoreHostname, collectionName);
			return manager.AsQueryable.FirstOrDefault();
		}

		public void WaitForChangeOnStoredObject<T>(Func<T, bool> func, int waitFor = 30) where T : class
		{
			var manager = new GenericRecordManager<T>(Kernel.Get<DroneSettings>().StoreHostname, null);
			var st = new Stopwatch();
			st.Start();

			while (!func(manager.Collection.FindOneAs<T>()) && st.ElapsedMilliseconds < waitFor * 1000)
			{
				Thread.Sleep(500);
			}

			func(manager.Collection.FindOneAs<T>()).Should().BeTrue();

			st.Stop();
		}

		public void StartDroneEndpoints()
		{
			var scheduler = Kernel.Get<IScheduler>();

			_nancy = new NancyHost(new Uri(Kernel.Get<DroneSettings>().BaseUrl), new DroneNancyNinjectBootstrapperForTesting(Kernel, scheduler));
			_nancy.Start();
		}

		public void StopDroneEndpoints()
		{
			if (_nancy == null)
				return;

			_nancy.Stop();
		}

		public void WaitForEmptyListOf<T>(string collectionName = null) where T : class
		{
			var manager = new GenericRecordManager<T>(Kernel.Get<DroneSettings>().StoreHostname, collectionName);

			var st = new Stopwatch();
			st.Start();

			while (st.ElapsedMilliseconds < 30 * 1000 && manager.Exists())
			{
				Thread.Sleep(500);
			}

			st.Stop();
			manager.Exists().Should().BeFalse("The collection had elements of type {0}", typeof(T).Name);
		}
	}
}