using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Acceptance.Framework;

namespace SpeedyMailer.Tests.Acceptance.RavenDB
{
    [TestFixture]
    public class RavenDbSanityTests : AcceptanceTestsBase
    {
        [Test]
        public void Sanity_ShouldReadAndWrite()
        {
            const string testingTheEmbeddedDb = "testing the embedded db";
            const string entityId = "entity1";

            using (var session = GetRavenDbDocumentStore().OpenSession())
            {
                session.Store(new ClassToStore()
                                  {
                                      TestingText = testingTheEmbeddedDb
                                  },entityId);
                session.SaveChanges();
            }

            using (var session = GetRavenDbDocumentStore().OpenSession())
            {
                var result = session.Load<ClassToStore>(entityId);

                result.TestingText.Should().Be(testingTheEmbeddedDb);

            }
        }

        private class ClassToStore
        {
            public string TestingText { get; set; }
        }
    }
}
