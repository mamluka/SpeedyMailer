using System.Diagnostics;
using EqualityComparer;
using NUnit.Framework;
using Ninject;
using Ninject.Activation.Strategies;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Master.Web.UI.Bootstrappers;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    [TestFixture]
    public class IntegrationTestBase
    {
        public IKernel DroneKernel { get; private set; }
        public StandardKernel MasterKernel { get; private set; }
        public IDocumentStore DocumentStore { get; private set; }

        public Actions Drone { get; set; }
        public Actions Master { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {

        }

        [SetUp]
        public void Setup()
        {
            var embeddableDocumentStore = new EmbeddableDocumentStore {RunInMemory = true};
            DocumentStore = embeddableDocumentStore.Initialize();
        }

        public void Store(object item)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
        }



        private void RebindToInMemeoryDatabase(IKernel kernel)
        {
            kernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
        }

        public T MasterResolve<T>()
        {
            return MasterKernel.Get<T>();
        }

        public T DroneResolve<T>()
        {
            return DroneKernel.Get<T>();
        }

        public bool Compare<T>(T first, T second)
        {
            return MemberComparer.Equal(first, second);
        }
    }
}