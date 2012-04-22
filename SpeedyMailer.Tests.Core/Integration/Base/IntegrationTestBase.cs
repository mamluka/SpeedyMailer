using System.Diagnostics;
using EqualityComparer;
using NUnit.Framework;
using Ninject;
using Ninject.Activation.Strategies;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Core.Container;
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
            var embeddableDocumentStore = new EmbeddableDocumentStore { RunInMemory = true };
            DocumentStore = embeddableDocumentStore.Initialize();

            MasterKernel = new StandardKernel();
            SetupMasterKernel(MasterKernel);

            DroneKernel = DroneNinjectBootstrapper.Kernel;

            RebindToInMemeoryDatabase(MasterKernel);

            Drone = new Actions(DroneKernel);
            Master = new Actions(MasterKernel);

        }

        private void SetupMasterKernel(StandardKernel masterKernel)
        {
            masterKernel.Bind<IDocumentStore>().ToConstant(DocumentStore);
            masterKernel.BindInterfaces(x => x.FromAssembliesMatching(new[] { "SpeedyMailer.Core" }))
                .BindSettingsToDocumentStoreFor(x => x.FromAssembliesMatching(new[] { "SpeedyMailer.Core" }));
            

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