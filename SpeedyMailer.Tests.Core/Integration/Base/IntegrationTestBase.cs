using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Quartz;
using Quartz.Impl.Matchers;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Server;
using Rhino.Mocks;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	[TestFixture]
	public class IntegrationTestBase : AutoMapperAndFixtureBase
	{
		public IKernel DroneKernel { get; private set; }

		public IKernel MasterKernel { get; private set; }

		protected string DefaultBaseUrl { get; set; }

		public IDocumentStore DocumentStore { get; private set; }

		public DroneActions DroneActions { get; set; }

		public UiActions UiActions { get; set; }

		public ServiceActions ServiceActions { get; set; }

		private readonly IntegrationTestsOptions _options = new IntegrationTestsOptions();

		public IntegrationStoreHelpers Store { get; set; }

		public IntegrationApiHelpers Api { get; set; }

		public IntegrationEmailHelpers Email { get; set; }

		public IntegrationTasksHelpers Tasks { get; set; }

		public IntegrationMongoDbHelper MongoDb { get; set; }

		protected static IList<object> EventRegistry { get; set; }

		public IntegrationTestBase()
		{

		}
		protected IntegrationTestBase(Action<IntegrationTestsOptions> action)
		{
			action(_options);
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			MasterKernel = ContainersConfigurationsForTesting.Service();
			DroneKernel = ContainersConfigurationsForTesting.Drone();

			MasterKernel.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InSingletonScope();
			DroneKernel.Rebind<IScheduler>().ToProvider<QuartzSchedulerProvider>().InSingletonScope();

			DefaultBaseUrl = IntergrationHelpers.GenerateRandomLocalhostAddress();

			IntergrationHelpers.ValidateSettingClasses();
		}

		[SetUp]
		public void Setup()
		{
			var embeddableDocumentStore = CreateEmbeddableStore();
			DocumentStore = embeddableDocumentStore.Initialize();

			MasterKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
			DroneKernel.Rebind<IDocumentStore>().ToConstant(ContainersConfigurationsForTesting.MockedDocumentStore);

			RegisterActions();
			ExtraSetup();
			ClearEventRegistry();

			Api = new IntegrationApiHelpers(DefaultBaseUrl);
			Store = new IntegrationStoreHelpers(DocumentStore);
			Email = new IntegrationEmailHelpers();
			Tasks = new IntegrationTasksHelpers(Store);
			MongoDb = new IntegrationMongoDbHelper();

			if (_options.UseMongo)
				MongoDb.StartMongo();
		}

		private void ClearEventRegistry()
		{
			EventRegistry = new List<object>();
		}

		private void DeleteLogs()
		{
			Directory.Delete("logs", true);
		}

		private static IDocumentStore CreateEmbeddableStore()
		{
			var store = new EmbeddableDocumentStore
							{
								RunInMemory = true,
								UseEmbeddedHttpServer = true,
								Conventions =
									{
										CustomizeJsonSerializer =
											serializer =>
											{
												serializer.TypeNameHandling = TypeNameHandling.All;
											},
										FindTypeTagName =
											type =>
											typeof(PersistentTask).IsAssignableFrom(type)
												? "persistenttasks"
												: DocumentConvention.DefaultTypeTagName(type),
									}
							};

			store.Initialize();
			Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(ServiceAssemblyMarker).Assembly, store);

			return store;
		}

		[TearDown]
		public void Teardown()
		{
			ServiceActions.Stop();
			Email.DeleteEmails();
			DeleteJsonSettingFiles();
			ClearJobsFromSchedulers();
			DroneActions.StopAllDroneStores();

			Api.StopListeningToApiCalls();

			if (_options.UseMongo)
				MongoDb.ShutdownMongo();

			ExtraTeardown();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			ClearJobsFromSchedulers();
			DeleteJsonSettingFiles();
		}

		private void ClearJobsFromSchedulers()
		{
			var masterScheduler = MasterResolve<IScheduler>();
			DeleteRunningJobs(masterScheduler);

			var droneScheduler = DroneResolve<IScheduler>();
			DeleteRunningJobs(droneScheduler);
		}

		private static void DeleteRunningJobs(IScheduler scheduler)
		{
			if (scheduler.IsShutdown)
				return;

			var groups = scheduler.GetJobGroupNames();

			var jobKeys = groups.SelectMany(x => scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));

			foreach (var key in jobKeys)
			{
				scheduler.PauseJob(key);
				scheduler.DeleteJob(key);
			}
		}

		private void DeleteJsonSettingFiles()
		{
			var settingFolders = Directory.GetDirectories(IntergrationHelpers.AssemblyDirectory).Where(x => x.Contains("settings_"));
			foreach (var settingFolder in settingFolders)
			{
				Directory.Delete(settingFolder, true);
			}
		}

		private void RegisterActions()
		{
			UiActions = MasterKernel.Get<UiActions>();
			ServiceActions = MasterKernel.Get<ServiceActions>();
			DroneActions = DroneKernel.Get<DroneActions>();
		}

		public virtual void ExtraSetup()
		{ }

		public virtual void ExtraTeardown()
		{ }

		public T MasterResolve<T>()
		{
			return MasterKernel.Get<T>();
		}

		public T DroneResolve<T>()
		{
			return DroneKernel.Get<T>();
		}

		protected void AssertEventWasPublished<T>(Action<T> shouldFunc) where T : class
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			while (stopWatch.ElapsedMilliseconds < 30 * 1000 && !EventRegistry.Contains(typeof(T)))
			{
				Thread.Sleep(250);
			}

			EventRegistry.Should().Contain(x=> x is T, "Event {0} did not fire", typeof(T).Name);
			shouldFunc(EventRegistry.First(x => x is T) as T);
			//testFunc().Should().BeTrue("Event {0} did not match the asserted values", typeof(T).Name);
		}

		protected void ListenToEvent<T>()
		{
			var generateMock = MockRepository.GenerateMock<HappendOn<T>>();
			generateMock.Stub(x => x.Inspect(Arg<T>.Is.Anything)).WhenCalled(x => EventRegistry.Add(x.Arguments[0]));
			DroneKernel.Bind<HappendOn<T>>().ToConstant(generateMock);
		}
	}
}