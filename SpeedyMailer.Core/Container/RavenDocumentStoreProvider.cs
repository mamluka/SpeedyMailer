using Newtonsoft.Json;
using Ninject.Activation;
using Raven.Client;
using Raven.Client.Document;
using SpeedyMailer.Core.Tasks;
using TypeNameHandling = Raven.Imports.Newtonsoft.Json.TypeNameHandling;

namespace SpeedyMailer.Core.Container
{
    public class RavenDocumentStoreProvider : Provider<IDocumentStore>
    {
        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new DocumentStore
	            {
					ConnectionStringName = "RavenDb",
					Conventions =
					{
						CustomizeJsonSerializer =
							serializer =>
							{
								serializer.TypeNameHandling = TypeNameHandling.All;
							},
						FindTypeTagName = type => typeof(PersistentTask).IsAssignableFrom(type) ? "persistenttasks" : DocumentConvention.DefaultTypeTagName(type),
						DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites
					}
	            };

            store.Initialize();
            return store;
        }
    }
}