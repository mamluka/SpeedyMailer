using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client.Embedded;
using Rhino.Mocks;
using FluentAssertions;

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
