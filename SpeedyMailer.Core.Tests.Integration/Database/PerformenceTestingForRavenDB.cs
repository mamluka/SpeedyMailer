using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.IntegrationTests.Database
{
    [TestFixture]
    public class PerformenceTestingForRavenDB : IntegrationTestBase
    {
        [Test]
        public void Store_WhenTryingToStoreAMillionContacts_ShouldMeasureTheTimeItTakes()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Enumerable.Range(1, 5).ToList().ForEach(i =>
                                                         {
                                                             var contacts =
                                                                 Fixture.Build<Contact>().Without(x => x.Id).CreateMany(
                                                                     5000);



                                                             using (var session = DocumentStore.OpenSession())
                                                             {
                                                                 contacts.ToList().ForEach(session.Store);

                                                                 session.SaveChanges();
                                                             }
                                                         }
                );

            stopWatch.Stop();

            Assert.Fail("Inserting took: " + stopWatch.ElapsedMilliseconds);
        }
    }
}
