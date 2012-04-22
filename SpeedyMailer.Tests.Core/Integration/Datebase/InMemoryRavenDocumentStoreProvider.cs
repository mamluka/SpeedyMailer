using Ninject.Activation;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpeedyMailer.Tests.Core.Integration.Datebase
{
    public class InMemoryRavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        protected override IDocumentStore CreateInstance(IContext context)
        {
            var ravenDbDocumentStore = new EmbeddableDocumentStore();
            ravenDbDocumentStore.Initialize();
            return ravenDbDocumentStore;
        }
    }
}