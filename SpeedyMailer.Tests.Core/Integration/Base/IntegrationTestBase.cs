using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using EqualityComparer;
using NUnit.Framework;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Ninject;
using Ninject.Activation;
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
using System.Linq;
using Nancy.ModelBinding;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    [TestFixture]
    public class IntegrationTestBase : AutoMapperAndFixtureBase
    {
    	private NancyHost _nancy;
    	public IKernel DroneKernel { get; private set; }
        public IKernel MasterKernel { get; private set; }
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
        }

        private void RegisterActions()
        {
            UIActions = MasterKernel.Get<UIActions>();
            ServiceActions = MasterKernel.Get<ServiceActions>();
            DroneActions = DroneKernel.Get<DroneActions>();
        }

        private void LoadBasicSettings()
        {
            UIActions.EditSettings<IBaseApiSettings>(x =>
                                                  {
                                                      x.ServiceBaseUrl = "http://localhost:2589";
                                                  });
            UIActions.EditSettings<ICreativeApisSettings>(x =>
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

		public void StartRestServer<T>(string endpoint)
		{
			var restCallTestingBootstrapper = new RestCallTestingBootstrapper(endpoint);
			_nancy = new NancyHost(new Uri("http://localhost:2589/"), restCallTestingBootstrapper);
			_nancy.Start();
		}

        public void AssertRestCall<T>(Func<T, bool> func) where T : class
        {
        	Assert.That(func(RestCallTestingModule.Model as T), Is.True);
        }

		public void StopRestServer()
		{
			_nancy.Stop();
		}
    }

    public class RestCallTestingModule : NancyModule
    {
        public static dynamic Model;

        public RestCallTestingModule(string baseUrl,string endpoint):base(baseUrl)
        {
            Get[endpoint] = x =>
                                {
                                    Model = this.Bind();
                                    return null;
                                };
        }
    }


    public class RestCallTestingBootstrapper : NinjectNancyBootstrapper
    {
    	private readonly Action<IKernel> _kernelAction;

		public RestCallTestingBootstrapper(string baseAndEndpoint)
    	{
    		_kernelAction = kernel =>
    		                	{
    		                		var endpoint = baseAndEndpoint.Split('/').Last();
    		                		var baseUrl = baseAndEndpoint.Substring(0, endpoint.Length - endpoint.Length);

    		                		kernel
    		                			.Bind<RestCallTestingModule>()
										.ToConstant(new RestCallTestingModule(baseUrl, endpoint));
    		                	};
    	}

    	protected override void ConfigureApplicationContainer(IKernel existingContainer)
    	{
    		_kernelAction(existingContainer);
    	}
    }
}