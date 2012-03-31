using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    public class IntergrationTestsBase
    {
        public EmbeddableDocumentStore RavenDbDocumentStore { get; set; }

        [SetUp]
        public void IntegrationStartup()
        {
            RavenDbDocumentStore = new EmbeddableDocumentStore
                                       {
                                           RunInMemory = true
                                       };

            RavenDbDocumentStore.Initialize();
        }

        public void Store(dynamic entity)
        {
            using (IDocumentSession session = RavenDbDocumentStore.OpenSession())
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