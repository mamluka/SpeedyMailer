using Ninject.Activation;
using Raven.Client;
using Raven.Client.Document;

namespace SpeedyMailer.Core.Container
{
    public class RavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new DocumentStore
	            {
					ConnectionStringName = "RavenDb"
	            };
            store.Initialize();
            return store;
        }
    }
}