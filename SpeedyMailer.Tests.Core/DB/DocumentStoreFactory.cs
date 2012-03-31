using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Rhino.Mocks;

namespace SpeedyMailer.Tests.Core.DB
{
    public static class DocumentStoreFactory
    {
        public static IDocumentStore StubDocumentStoreWithSession(IDocumentSession session)
        {
            var store = MockRepository.GenerateStub<IDocumentStore>();


            store.Stub(x => x.OpenSession()).Return(session);
            return store;
        }

        public static IDocumentStore StubDocumentStoreWithStubSession()
        {
            var session = MockRepository.GenerateStub<IDocumentSession>();

           return StubDocumentStoreWithSession(session);
        }
    }
}
