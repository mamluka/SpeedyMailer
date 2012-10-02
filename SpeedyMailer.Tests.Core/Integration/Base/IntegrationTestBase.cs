using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
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
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Tests.Core.Integration.Emails;
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

        protected string DefaultBaseUrl { get; set; }

        public IDocumentStore DocumentStore { get; private set; }
        public DroneActions DroneActions { get; set; }
        public UiActions UIActions { get; set; }
        public ServiceActions ServiceActions { get; set; }

        public string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

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
                .Configure(x => x.InTransientScope())
                .Storage<IDocumentStore>(x => x.Constant(DocumentStore))
                .Settings(x => x.UseDocumentDatabase())
                .Done();

            DroneKernel = ContainerBootstrapper
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

            DefaultBaseUrl = "http://localhost:" + DateTime.Now.Second + DateTime.Now.Millisecond;
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
                                                        FindTypeTagName = type => typeof(PersistentTask).IsAssignableFrom(type) ? "persistenttasks" : DocumentConvention.DefaultTypeTagName(type),
                                                        DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites
                                                    }
                                            };

            DocumentStore = embeddableDocumentStore.Initialize();
            MasterKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
            DroneKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);

            RegisterActions();
            ExtraSetup();
        }

        [TearDown]
        public void Teardown()
        {
            ServiceActions.Stop();
            DeleteEmails();
            StopSchedulers();
            StopListeningToApiCalls();
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
            WaitForSchedulerToShutdown(masterScheduler);

            var droneScheduler = MasterResolve<IScheduler>();
            WaitForSchedulerToShutdown(droneScheduler);
        }

        private void DeleteJsonSettingFiles()
        {
            var settingFolders = Directory.GetDirectories(AssemblyDirectory).Where(x => x.StartsWith("settings_"));
            foreach (var settingFolder in settingFolders)
            {
                Directory.Delete(settingFolder);
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
            UIActions = MasterKernel.Get<UiActions>();
            ServiceActions = MasterKernel.Get<ServiceActions>();
            DroneActions = DroneKernel.Get<DroneActions>();
        }

        public virtual void ExtraSetup()
        { }

        public virtual void ExtraTeardown()
        { }

        public void Store(object item)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
        }

        public void Store(object item, string id)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(item, id);
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
                return session.Query<T>().Where(expression).ToList();
            }
        }

        public IList<T> Query<T>()
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<T>().ToList();
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

        protected void WaitForEntitiesToExist<T>(int numberOfEntities, int secondsToWait = 30)
        {
            Func<IDocumentSession, Stopwatch, bool> condition =
                (session, stopwatch) =>
                session.Query<T>().Count() < numberOfEntities &&
                stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

            WaitForStoreWithFunction(condition);
        }
        protected void WaitForEntityToExist<T>(Func<T, bool> whereCondition, int count = 1, int secondsToWait = 30)
        {
            Func<IDocumentSession, Stopwatch, bool> condition =
                (session, stopwatch) =>
                session.Query<T>().Where(whereCondition).Count() < count &&
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

        public void ListenToApiCall<TEndpoint>(string endpointBaseUrl = "") where TEndpoint : ApiCall, new()
        {
            StartGeneticEndpoint<TEndpoint, NoResponse>(endpointBaseUrl, new NoResponse());
        }

        private void StartGeneticEndpoint<TEndpoint, TResponse>(string endpointBaseUrl, TResponse response)
            where TEndpoint : ApiCall, new()
            where TResponse : class
        {
            var endpoint = new TEndpoint().Endpoint;
            var uri = DefaultBaseUrl ?? endpointBaseUrl;
            RestCallTestingModule<TEndpoint, TResponse>.Model = default(TEndpoint);
            RestCallTestingModule<TEndpoint, TResponse>.WasCalled = false;
            RestCallTestingModule<TEndpoint, TResponse>.Files = null;

            var restCallTestingBootstrapper = new RestCallTestingBootstrapper<TEndpoint, TResponse>(endpoint, response);
            _nancy = new NancyHost(new Uri(uri), restCallTestingBootstrapper);
            _nancy.Start();

            Trace.WriteLine("Nancy started on: " + uri);
        }

        protected void PrepareApiResponse<TEndpoint, TResponse>(Action<TResponse> action, string endpointBaseUrl = "")
            where TResponse : class, new()
            where TEndpoint : ApiCall, new()
        {
            var response = new TResponse();
            action(response);
            StartGeneticEndpoint<TEndpoint, TResponse>(endpointBaseUrl, response);
        }

        public void AssertApiCalled<TEndpoint>(Func<TEndpoint, bool> func, int seconds = 30) where TEndpoint : class
        {
            WaitForApiToBeCalled<TEndpoint>(seconds);

            if (RestCallTestingModule<TEndpoint, NoResponse>.Model != null)
            {
                Assert.That(func(RestCallTestingModule<TEndpoint, NoResponse>.Model), Is.True);
                return;
            }
            Assert.Fail("REST call was not executed in the ellapsed time");
        }

        public void AssertFilesUploaded<TEndpoint>(IEnumerable<string> files, int seconds = 30) where TEndpoint : class
        {
            WaitForApiToBeCalled<TEndpoint>(seconds);

            if (RestCallTestingModule<TEndpoint, NoResponse>.Files != null)
            {
                Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.Files, Is.EquivalentTo(files.ToArray()));
                return;
            }

            Assert.Fail("REST call did not recieve the files");
        }

        public void AssertApiCalled<TEndpoint>(int seconds = 30) where TEndpoint : class
        {
            WaitForApiToBeCalled<TEndpoint>(seconds);

            Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.WasCalled, Is.True);
        }

        public void AssertApiWasntCalled<TEndpoint>(int seconds = 30) where TEndpoint : class
        {
            WaitForApiToBeCalled<TEndpoint>(seconds);

            Assert.That(RestCallTestingModule<TEndpoint, NoResponse>.WasCalled, Is.False);
        }

        public void AssertApiWasntCalled(int seconds = 30)
        {
            WaitForApiToBeCalled<NoResponse>(seconds);

            Assert.That(RestCallTestingModule<NoRequest, NoResponse>.WasCalled, Is.False);
        }

        private void WaitForApiToBeCalled<TEndpoint>(int seconds) where TEndpoint : class
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (RestCallTestingModule<TEndpoint, NoResponse>.WasCalled == false && stopwatch.ElapsedMilliseconds < seconds * 1000)
            {
                Thread.Sleep(500);
            }
        }

        public void StopListeningToApiCalls()
        {
            if (_nancy == null)
                return;

            _nancy.Stop();

            Trace.WriteLine("Nancy stopped on: " + DefaultBaseUrl);
        }

        protected void AssertEmailSent(Func<FakeEmailMessage, bool> func, int waitFor = 30)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var files = new List<string>();

            while (!files.Any() && stopwatch.ElapsedMilliseconds < waitFor * 1000)
            {
                files = GetEmailFiles().ToList();
                Thread.Sleep(500);
            }

            if (!files.Any())
                Assert.Fail("No emails were found");

            var email = JsonConvert.DeserializeObject<FakeEmailMessage>(File.ReadAllText(files.First()));
            Assert.That(func(email));
        }

        protected void AssertEmailSent(Action<FakeEmailMessage> action, int waitFor = 30)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var files = new List<string>();

            while (!files.Any() && stopwatch.ElapsedMilliseconds < waitFor * 1000)
            {
                files = GetEmailFiles().ToList();
                Thread.Sleep(500);
            }

            if (!files.Any())
                Assert.Fail("No emails were found");

            var email = JsonConvert.DeserializeObject<FakeEmailMessage>(File.ReadAllText(files.First()));
            action(email);
        }

        protected void DeleteEmails()
        {
            GetEmailFiles().ToList().ForEach(File.Delete);
        }

        private IEnumerable<string> GetEmailFiles()
        {
            return Directory.GetFiles(AssemblyDirectory).Where(x =>
                                                                {
                                                                    var filename = Path.GetFileName(x);
                                                                    if (string.IsNullOrEmpty(filename))
                                                                        return false;

                                                                    return filename.StartsWith("email") &&
                                                                           filename.EndsWith(".persist");
                                                                });
        }
    }

    public class RestCallTestingModule<TEndpoint, TResponse> : NancyModule, IDoNotResolveModule where TResponse : class
    {
        public static TEndpoint Model;
        public static bool WasCalled;
        public static IList<string> Files;

        public RestCallTestingModule(string endpoint, TResponse response)
            
        {
            Get[endpoint] = x => RecordResponse(response);

            Post[endpoint] = x => RecordResponse(response);

            Put[endpoint] = x => RecordResponse(response);
        }

        private Response RecordResponse(TResponse response)
        {
            Model = this.Bind<TEndpoint>();
            Files = Request.Files.Select(file => file.Name).ToList();
            WasCalled = true;

            return response != null ? Response.AsJson(response) : Response.AsText("OK");
        }
    }

    public class RestCallTestingBootstrapper<TEndpoint, TResponse> : NinjectNancyBootstrapper where TResponse : class
    {
        private readonly string _baseAndEndpoint;
        private readonly TResponse _response;

        public RestCallTestingBootstrapper(string baseAndEndpoint, TResponse response)
        {
            _response = response;
            _baseAndEndpoint = baseAndEndpoint;
        }

        protected override void RegisterRequestContainerModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            container.Bind<NancyModule>()
                .ToConstant(new RestCallTestingModule<TEndpoint, TResponse>(_baseAndEndpoint, _response))
                .Named(GetModuleKeyGenerator().GetKeyForModuleType(typeof(RestCallTestingModule<TEndpoint, TResponse>)));
        }

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration.WithOverrides(
					   c => c.Serializers.Insert(0, typeof(JsonNetSerializer)));
			}
		}
    }

    public class NoRequest
    {
    }

    public class NoResponse
    {

    }

}