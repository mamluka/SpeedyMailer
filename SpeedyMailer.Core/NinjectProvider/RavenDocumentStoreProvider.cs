using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Activation;
using Raven.Client;
using Raven.Client.Document;

namespace SpeedyMailer.Core.NinjectProvider
{
    public class RavenDocumentStoreProvider : Provider<IDocumentStore>
    {


        protected override IDocumentStore CreateInstance(IContext context)
        {
            var store = new DocumentStore {Url = "http://localhost:8080"};
            
            
            store.Initialize();
            return store;
        }
    }
}
