using FluentAssertions;
using NUnit.Framework;
using Raven.Client;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Tests.Acceptance.RavenDB
{
    [TestFixture]
    public class RavenDbSanityTests : IntegrationTestBase
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

            using (var session = RavenDbDocumentStore.OpenSession())
            {
                session.Store(new ClassToStore
                                  {
                                      TestingText = testingTheEmbeddedDb
                                  }, entityId);
                session.SaveChanges();
            }

            using (var session = RavenDbDocumentStore.OpenSession())
            {
                var result = session.Load<ClassToStore>(entityId);

                result.TestingText.Should().Be(testingTheEmbeddedDb);
            }
        }
    }
}