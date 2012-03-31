using NUnit.Framework;
using Ninject;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Core.NinjectProvider;
using SpeedyMailer.Core.Container;
using FluentAssertions;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    [TestFixture]
    public class ExtentionsTests : IntergrationTestsBase
    {
        private StandardKernel _targert;

        [SetUp]
        public void Setup()
        {
            _targert = new StandardKernel();
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

        private void BindStoreToContainer()
        {
            _targert.Bind<IDocumentStore>().ToConstant(RavenDbDocumentStore);
        }

        public interface ITestingSettings
        {
            [Default("David")]
            string Name { get; set; }
        }
    }

    public class IntergrationTestsBase
    {
        public EmbeddableDocumentStore RavenDbDocumentStore { get; set; }

        [SetUp]
        public void IntegrationStartup()
        {
            RavenDbDocumentStore = new EmbeddableDocumentStore()
                                       {
                                           RunInMemory = true
                                       };

            RavenDbDocumentStore.Initialize();
        }

        public void Store(dynamic entity)
        {
            using (var session = RavenDbDocumentStore.OpenSession())
            {
                session.Store(entity);
                session.SaveChanges();
            }
        }
        [TearDown]
        public void IntegrationTearDown()
        {
            RavenDbDocumentStore.Dispose();
        }
    }
}
