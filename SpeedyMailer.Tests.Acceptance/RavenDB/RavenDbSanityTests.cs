using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using SpeedyMailer.Tests.Acceptance.Framework;

namespace SpeedyMailer.Tests.Acceptance.RavenDB
{
    [TestFixture]
    public class RavenDbSanityTests : AcceptanceTestsBase
    {
        private class ClassToStore
        {
            public string TestingText { get; set; }
        }

        [Test]
        public void Sanity_ShouldReadAndWrite()
        {
            const string testingTheEmbeddedDb = "testing the embedded db";
            const string entityId = "entity1";

            using (IDocumentSession session = GetRavenDbDocumentStore().OpenSession())
            {
                session.Store(new ClassToStore
                                  {
                                      TestingText = testingTheEmbeddedDb
                                  }, entityId);
                session.SaveChanges();
            }

            using (IDocumentSession session = GetRavenDbDocumentStore().OpenSession())
            {
                var result = session.Load<ClassToStore>(entityId);

                result.TestingText.Should().Be(testingTheEmbeddedDb);
            }
        }
    }
}