using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Linq;
using EqualityComparer;
using NUnit.Framework;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Ninject;
using Quartz;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using RestSharp;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drone;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Shared;
using SpeedyMailer.Tests.Core.Unit.Base;
using Raven.Client.Linq;
using Nancy.ModelBinding;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	[TestFixture]
	public class IntegrationTestBase : AutoMapperAndFixtureBase
	{
		private NancyHost _nancy;
		public IKernel DroneKernel { get; private set; }
		public IKernel MasterKernel { get; private set; }
		protected string ApiListningHostname { set; get; }

		public IDocumentStore DocumentStore { get; private set; }

		public DroneActions DroneActions { get; set; }
		public UIActions UIActions { get; set; }
		public ServiceActions ServiceActions { get; set; }


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
                                                            typeof (SharedAssemblyMarker),
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
                                                            typeof (SharedAssemblyMarker),
                                                            typeof (ISchedulerFactory),
                                                             typeof (CoreAssemblyMarker)
                                                        }))
				.BindInterfaceToDefaultImplementation()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			ApiListningHostname = "http://localhost:2589/";
		}

		private void RegisterActions()
		{
			UIActions = MasterKernel.Get<UIActions>();
			ServiceActions = MasterKernel.Get<ServiceActions>();
			DroneActions = DroneKernel.Get<DroneActions>();
		}

		private void LoadBasicSettings()
		{
			UIActions.EditSettings<IApiCallsSettings>(x=> x.ApiBaseUri = ApiListningHostname);
			DroneActions.EditSettings<IApiCallsSettings>(x=> x.ApiBaseUri = ApiListningHostname);
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
		protected void WaitForEntityToExist<T>(Func<T,bool> whereCondition ,int count=1, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Query<T>().Customize(x => x.WaitForNonStaleResults()).Where(whereCondition).Count() < count &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}

		protected void WaitForTaskToComplete(string taskId, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Load<PersistentTask>(taskId).Status == PersistentTaskStatus.Executed &&
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

		public void ListenToApiCall<TEndpoint, TResponse>() where TEndpoint : ApiCall, new()
		{
			var endpoint = new TEndpoint().Endpoint;
			var restCallTestingBootstrapper = new RestCallTestingBootstrapper<TResponse>(endpoint);
			_nancy = new NancyHost(new Uri(ApiListningHostname), restCallTestingBootstrapper);
			_nancy.Start();
		}

		public void AssertApiCall<T>(Func<T, bool> func,int seconds = 30) where T : class
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (RestCallTestingModule<T>.Model == null && stopwatch.ElapsedMilliseconds < seconds*1000)
			{
				Thread.Sleep(500);
			}

			StopListeningToApiCall();

			if (RestCallTestingModule<T>.Model != null)
			{
				Assert.That(func(RestCallTestingModule<T>.Model), Is.True);
				return;
			}
			Assert.Fail("Rest call was not executed in the ellapsed time");
			
		}

		public void StopListeningToApiCall()
		{
			_nancy.Stop();
		}
	}

	public class RestCallTestingModule<T> : NancyModule,IDoNotResolveModule
	{
		public static T Model;

		public RestCallTestingModule(string baseBaseUrl, string endpoint)
			: base(baseBaseUrl)
		{
			Get[endpoint] = x =>
								{
									Model = this.Bind<T>();
									return null;
								};

			Post[endpoint] = x =>
			                 	{
			                 		Model = this.Bind<T>();
			                 		return null;
			                 	};
		}
	}

	public class RestCallTestingBootstrapper<T> : NinjectNancyBootstrapper
	{
		private readonly string _baseAndEndpoint;

		public RestCallTestingBootstrapper(string baseAndEndpoint)
		{
			_baseAndEndpoint = baseAndEndpoint;
		}

		protected override void RegisterRequestContainerModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
		{
			var endpoint = "/" + _baseAndEndpoint.Split('/').Last();
			var baseUrl = _baseAndEndpoint.Substring(0, _baseAndEndpoint.Length - endpoint.Length);

			container.Bind<NancyModule>()
				.ToConstant(new RestCallTestingModule<T>(baseUrl, endpoint))
				.Named(GetModuleKeyGenerator().GetKeyForModuleType(typeof(RestCallTestingModule<T>)));
		}
	}
}