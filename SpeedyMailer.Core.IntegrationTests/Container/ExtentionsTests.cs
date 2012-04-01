using FluentAssertions;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    [TestFixture]
    public class ExtentionsTests : IntergrationTestsBase
    {
        [SetUp]
        public void Setup()
        {
            _target = new StandardKernel();
        }

        private StandardKernel _target;

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
            _target.BindSettingsToJsonFilesFor(x => x.FromThisAssembly());

            var result = _target.Get<ITestingSettings>();
            result.Name.Should().Be("Moshe");
        }
    }
}