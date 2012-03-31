using NUnit.Framework;
using Raven.Client.Embedded;

namespace SpeedyMailer.Tests.Acceptance.Framework
{
    [TestFixture]
    public class AcceptanceTestsBase
    {
        private EmbeddableDocumentStore _ravenDbDocumentStore;


        public EmbeddableDocumentStore GetRavenDbDocumentStore()
        {
            return _ravenDbDocumentStore;
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _ravenDbDocumentStore = new EmbeddableDocumentStore();
            _ravenDbDocumentStore.Initialize();
        }
    }
}