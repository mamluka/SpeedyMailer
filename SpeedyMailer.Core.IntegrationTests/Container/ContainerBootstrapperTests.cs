using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Ninject.Activation;
using Ploeh.AutoFixture;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    [TestFixture]
    public class ContainerBootstrapperTests : IntegrationTestBase
    {
        [Test]
        public void Bootstrap_WhenAnalyzingAnAssemblyUsingAGivenType_ShouldResolveTypesFromThatAssembly()
        {
            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[]{typeof(TestAssemblyMarkerType)}))
                .BindInterfaceToDefaultImplementation()
                .Done();

            var result = kernel.Get<ITestingInterface>();

            result.GetType().Should().Be<TestingInterface>();
        }

        [Test]
        public void Bootstrap_WhenAnalyzingAnAssemblyUsingAGivenType_ShouldNotResolveOtherAssemblies()
        {
            Assert.Throws<ActivationException>(() =>
                                                   {
                                                       var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(ServiceAssemblyMarker) }))
                .BindInterfaceToDefaultImplementation()
                .Done();

                                                       var result = kernel.Get<ITestingInterface>();
                                                   });
        }

        [Test]
        public void Bootstrap_WhenBindingToStore_ShouldResovleIt()
        {
            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Provider<TestDatabaseProvider>())
                .Done();

            var result = kernel.Get<IDocumentStore>();

            result.GetType().Should().Be<EmbeddableDocumentStore>();
        }

        [Test]
        public void Bootstrap_WhenBindingToConstantDatabase_ShouldResolveToThatConstant()
        {
            var store = new EmbeddableDocumentStore
                            {
                                RunInMemory = true
                            };
            store.Initialize();

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Constant(store))
                .Done();

            var result = kernel.Get<IDocumentStore>();

            result.GetType().Should().Be<EmbeddableDocumentStore>();
        }

        [Test]
        public void Bootstrap_WhenBindingSettingsToStore_ShouldResolveSettingsUsingStore()
        {
            var entity = new
            {
                Id = "settings/Testing",
                JustSomeProperty = "from-store"
            };

            Store(entity);

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Constant(DocumentStore))
                .Settings(x=> x.UseDocumentDatabase())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("from-store");

        }

        [Test]
        public void Bootstrap_WhenSettingsDoesntExistInStore_ShouldReturnDefaultValues()
        {
            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Constant(DocumentStore))
                .Settings(x => x.UseDocumentDatabase())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("default");
        }

        [Test]
        public void Bootstrap_WhenSettingsExistInStoreButThePropertyDoesnt_ShouldReturnDefaultValues()
        {
            var entity = new
            {
                Id = "settings/Testing",
            };
            Store(entity);

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Constant(DocumentStore))
                .Settings(x => x.UseDocumentDatabase())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("default");
        }

        [Test]
        public void Bootstrap_WhenBindingSettingsToJsonFiles_ShouldResolveSettingsUsingStore()
        {
            var entity = new
            {
                JustSomeProperty = "from-json"
            };

            CreateJsonSettingsFile(entity);

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .NoDatabase()
                .Settings(x => x.UseJsonFiles())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("from-json");

        }

        [Test]
        public void Bootstrap_WhenSettingsDoesntExistInJsonFiles_ShouldReturnDefaultValues()
        {
            DeleteJsonFile();

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .NoDatabase()
                .Settings(x => x.UseJsonFiles())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("default");
        }

        [Test]
        public void Bootstrap_WhenSettingsExistInJsonButThePropertyDoesnt_ShouldReturnDefaultValues()
        {
            var entity = new
            {
            };

            CreateJsonSettingsFile(entity);

            var kernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
                .BindInterfaceToDefaultImplementation()
                .NoDatabase()
                .Settings(x => x.UseDocumentDatabase())
                .Done();

            var result = kernel.Get<ITestingSettings>();

            result.JustSomeProperty.Should().Be("default");
        }

        private void CreateJsonSettingsFile(dynamic setting)
        {
            using (var writter = new StreamWriter("settings/Testing.settings"))
            {
                writter.WriteLine(JsonConvert.SerializeObject(setting));
            }
        }

        private void DeleteJsonFile()
        {
            File.Delete("settings/Testing.settings");
        }

    }

    
    public interface ITestingSettings
    {
        [Default("default")]
        string JustSomeProperty { get; set; }
    }

    public class TestAssemblyMarkerType
    {}

    public interface ITestingInterface
    {}

    public class TestingInterface : ITestingInterface
    {}

    public class TestDatabaseProvider:Provider<IDocumentStore>
    {
        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new EmbeddableDocumentStore
                            {
                                RunInMemory = true
                            };
            return store.Initialize();

        }
    }
}
