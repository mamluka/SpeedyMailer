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
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _targert = new StandardKernel();
        }

        #endregion

        private StandardKernel _targert;

        private void BindStoreToContainer()
        {
            _targert.Bind<IDocumentStore>().ToConstant(RavenDbDocumentStore);
        }

        public interface ITestingSettings
        {
            [Default("David")]
            string Name { get; set; }
        }

        private class TestingSettings : ITestingSettings
        {
            #region ITestingSettings Members

            public string Name { get; set; }

            #endregion
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
            _targert.BindInterfaces(x => x.FromThisAssembly());
            var result = _targert.Get<IDefaultClass>();

            result.GetType().Name.Should().Be("DefaultClass");
        }

        [Test]
        public void BindInterfaces_ShouldNotBindSettingsInterfacesToInstences()
        {
            Assert.Throws<ActivationException>(() =>
                                                   {
                                                       _targert.BindInterfaces(x => x.FromThisAssembly());
                                                       _targert.Get<ITestingSettings>();
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

            _targert.BindSettingsFor(x => x.FromThisAssembly());

            var result = _targert.Get<ITestingSettings>();
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

            _targert.BindSettingsFor(x => x.FromThisAssembly());

            var result = _targert.Get<ITestingSettings>();
            result.Name.Should().Be("David");
        }

        [Test]
        public void BindSettingsFor_ShouldReplaceWithDefaultWhenObjectIsMissing()
        {
            BindStoreToContainer();
            _targert.BindSettingsFor(x => x.FromThisAssembly());

            var result = _targert.Get<ITestingSettings>();
            result.Name.Should().Be("David");
        }
    }
}