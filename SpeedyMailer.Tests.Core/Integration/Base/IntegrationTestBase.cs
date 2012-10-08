using System;
using System.IO;
using System.Threading;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Util;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using SpeedyMailer.Core.Tasks;
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

			Api = new IntegrationApiHelpers(DefaultBaseUrl);
			Store = new IntegrationStoreHelpers(DocumentStore);
			Email = new IntegrationEmailHelpers();

			if (_options.UseMongo)
				DroneActions.StartMongo();
		}

		private static IDocumentStore CreateEmbeddableStore()
		{
			var store = new EmbeddableDocumentStore
							{
								RunInMemory = true,
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
										DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites
									}
							};

			store.Initialize();
			return store;
		}

		[TearDown]
		public void Teardown()
		{
			ServiceActions.Stop();
			Email.DeleteEmails();
			DeleteJsonSettingFiles();
			StopSchedulers();

			Api.StopListeningToApiCalls();

			if (_options.UseMongo)
				DroneActions.ShutdownMongo();

			ExtraTeardown();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			StopSchedulers();
			DeleteJsonSettingFiles();
		}

		private void StopSchedulers()
		{
			var masterScheduler = MasterResolve<IScheduler>();
			DeleteRunningJobs(masterScheduler);

			var droneScheduler = DroneResolve<IScheduler>();
			DeleteRunningJobs(droneScheduler);
		}

		private static void DeleteRunningJobs(IScheduler masterScheduler)
		{
			var groups = masterScheduler.GetJobGroupNames();
			var jobKeys = groups.SelectMany(x => masterScheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));

			foreach (var key in jobKeys)
			{
				masterScheduler.PauseJob(key);
				masterScheduler.DeleteJob(key);
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

		private static void WaitForSchedulerToShutdown(IScheduler scheduler)
		{
			if (scheduler.IsStarted)
			{
				scheduler.Shutdown();

				while (!scheduler.IsShutdown)
				{
					Thread.Sleep(200);
				}
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




	}
}