using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using EqualityComparer;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Quartz;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Master.Web.UI;
using SpeedyMailer.Tests.Core.Unit.Base;
using Raven.Client.Linq;
using System.Linq;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	[TestFixture]
	public class IntegrationTestBase : AutoMapperAndFixtureBase
	{
		public IKernel DroneKernel { get; private set; }
		public IKernel MasterKernel { get; private set; }
		public IDocumentStore DocumentStore { get; private set; }

		public DroneActions Drone { get; set; }
		public Actions UI { get; set; }
		public MasterActions Master { get; set; }


		[TestFixtureSetUp]
		public void FixtureSetup()
		{

			DocumentStore = MockRepository.GenerateStub<IDocumentStore>();

			MasterKernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (CoreAssemblyMarker),
                                                            typeof (WebCoreAssemblyMarker),
                                                            typeof (ServiceAssemblyMarker),
                                                            typeof (IRestClient),
                                                            typeof (ISchedulerFactory)
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			DroneKernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (DroneAssemblyMarker),
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();
		}

		private void RegisterActions()
		{
			UI = MasterKernel.Get<Actions>();
			Master = MasterKernel.Get<MasterActions>();
			Drone = DroneKernel.Get<DroneActions>();
		}

		private void LoadBasicSettings()
		{
			UI.EditSettings<IBaseApiSettings>(x =>
												  {
													  x.ServiceBaseUrl = "http://localhost:2589";
												  });
			UI.EditSettings<ICreativeApisSettings>(x =>
													   {
														   x.AddCreative = "/creative/add";
													   });
		}

		[SetUp]
		public void Setup()
		{
			var embeddableDocumentStore = new EmbeddableDocumentStore
											{
												RunInMemory = true,
												Conventions =
													{
														CustomizeJsonSerializer =
															serializer =>
															{
																serializer.TypeNameHandling = TypeNameHandling.All;
															},
														FindTypeTagName = type => typeof(PersistentTask).IsAssignableFrom(type) ? "PersistentTasks" : DocumentConvention.DefaultTypeTagName(type),

													}
											};

			DocumentStore = embeddableDocumentStore.Initialize();

			MasterKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
			DroneKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);

			RegisterActions();
			LoadBasicSettings();
			ExtraSetup();
		}

		public virtual void ExtraSetup()
		{ }

		public void Store(object item)
		{
			using (var session = DocumentStore.OpenSession())
			{
				session.Store(item);
				session.SaveChanges();
			}
		}


		public T MasterResolve<T>()
		{
			return MasterKernel.Get<T>();
		}

		public T DroneResolve<T>()
		{
			return DroneKernel.Get<T>();
		}

		public bool Compare<T>(T first, T second)
		{
			return MemberComparer.Equal(first, second);
		}

		public T Load<T>(string id)
		{
			using (var session = DocumentStore.OpenSession())
			{
				return session.Load<T>(id);
			}
		}

		public IList<T> Query<T>(Expression<Func<T, bool>> expression)
		{
			using (var session = DocumentStore.OpenSession())
			{
				return session.Query<T>().Where(expression).Customize(x => x.WaitForNonStaleResults()).ToList();
			}
		}

		public IList<T> Query<T>()
		{
			using (var session = DocumentStore.OpenSession())
			{
				return session.Query<T>().Customize(x => x.WaitForNonStaleResults()).ToList();
			}
		}

		protected void Delete<T>(string entityId)
		{
			using (var session = DocumentStore.OpenSession())
			{
				var entity = session.Load<T>(entityId);
				session.Delete(entity);
				session.SaveChanges();
			}
		}

		protected void WaitForEntityToExist(string entityId, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
					session.Load<object>(entityId) == null &&
					stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}

		protected void WaitForEntityToExist<T>(int numberOfEntities, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Query<T>().Customize(x => x.WaitForNonStaleResults()).Count() > numberOfEntities &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}

		private void WaitForStoreWithFunction(Func<IDocumentSession, Stopwatch, bool> condition)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			using (var session = DocumentStore.OpenSession())
			{
				session.Advanced.MaxNumberOfRequestsPerSession = 200;
				while (condition(session, stopwatch))
				{
					Thread.Sleep(500);
				}
			}
		}
	}
}