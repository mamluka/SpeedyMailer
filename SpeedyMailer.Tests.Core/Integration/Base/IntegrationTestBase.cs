using NUnit.Framework;
using Ninject;
using Raven.Client.Embedded;
using SpeedyMailer.Master.Service.Core.Container;
using SpeedyMailer.Master.Web.UI.Bootstrappers;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    [TestFixture]
    public class IntegrationTestBase
    {
        public IKernel DroneKernel { get; private set; }
        public StandardKernel MasterKernel { get; private set; }
        public EmbeddableDocumentStore RavenDbDocumentStore { get; private set; }

        public Actions Drone { get; set; }
        public Actions Master { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            RavenDbDocumentStore = new EmbeddableDocumentStore();
            RavenDbDocumentStore.Initialize();

            MasterKernel = new StandardKernel();
            var masterContainer = new MasterNinjectBootstrapper();
            masterContainer.Register(MasterKernel);
            DroneKernel = DroneNinjectBootstrapper.Kernel;

            Drone = new Actions(DroneKernel);
            Master = new Actions(MasterKernel);

        }

        public T MasterResolve<T>()
        {
            return MasterKernel.Get<T>();
        }

        public T DroneResolve<T>()
        {
            return DroneKernel.Get<T>();
        }
    }
}