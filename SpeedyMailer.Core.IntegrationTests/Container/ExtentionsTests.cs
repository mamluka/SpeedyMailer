using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Raven.Client;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    [TestFixture]
    public class ExtentionsTests : IntergrationTestsBase
    {
        private StandardKernel _target;
        
        [SetUp]
        public void Setup()
        {
            _target = new StandardKernel();
        }
        [TearDown]
        public void Teardown()
        {
            DeleteJsonFile();
        }

        private void DeleteJsonFile()
        {
            File.Delete("settings/Testing.settings");
        }

        private void BindStoreToContainer()
        {
            _target.Bind<IDocumentStore>().ToConstant(RavenDbDocumentStore);
        }

        public interface ITestingSettings
        {
            [Default("David")]
            string Name { get; set; }
        }

        private class TestingSettings : ITestingSettings
        {
            public string Name { get; set; }
        }

        public interface IDefaultClass
        {
        }

        public class DefaultClass : IDefaultClass
        {
        }

        [Test]
        public void BindInterfaces_ShouldBindInterfaceToItsDefaultImplementation()
        {
            _target.BindInterfaces(x => x.FromThisAssembly());
            var result = _target.Get<IDefaultClass>();

            result.GetType().Name.Should().Be("DefaultClass");
        }

        [Test]
        public void BindInterfaces_ShouldNotBindSettingsInterfacesToInstences()
        {
            Assert.Throws<ActivationException>(() =>
                                                   {
                                                       _target.BindInterfaces(x => x.FromThisAssembly());
                                                       _target.Get<ITestingSettings>();
                                                   });
        }

        [Test]
        public void BindSettingsFor_ShouldReadTheConfigurationFromTheDatabaseWhenExist()
        {
            var entity = new
                             {
                                 Id = "settings/Testing",
                                 Name = "Moshe"
                             };

            Store(entity);
            BindStoreToContainer();

            _target.BindSettingsToDocumentStoreFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("Moshe");
        }

        [Test]
        public void BindSettingsFor_ShouldReplaceWithDefaulValueIfMissingButWholeObjectExists()
        {
            var entity = new
                             {
                                 Id = "settings/Testing",
                             };

            Store(entity);
            BindStoreToContainer();

            _target.BindSettingsToDocumentStoreFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("David");
        }

        [Test]
        public void BindSettingsFor_ShouldReplaceWithDefaultWhenObjectIsMissing()
        {
            BindStoreToContainer();
            _target.BindSettingsToDocumentStoreFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("David");
        }

        [Test]
        public void BindSettingsToJsonFilesFor_ShouldLoadSettingFromJsonFileIfExists()
        {
            CreateJsonSettingsFile(new
                                       {
                                           Name="Moshe"
                                       });

            _target.BindSettingsToJsonFilesFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("Moshe");
        }

        [Test]
        public void BindSettingsToJsonFilesFor_ShouldReadSettingsFromDefaultAttrWhenFileDoesntExist()
        {
            _target.BindSettingsToJsonFilesFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("David");
        }

        private void CreateJsonSettingsFile(dynamic setting)
        {
            using (var writter = new  StreamWriter("settings/Testing.settings"))
            {
                writter.WriteLine(JsonConvert.SerializeObject(setting));
            }
        }
    }
}