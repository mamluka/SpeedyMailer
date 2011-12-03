using System.Collections.Generic;
using Raven.Client;
using System.Linq;
namespace SpeedyMailer.Core.Lists
{
    public class ListRepository : IListRepository
    {
        private readonly IDocumentStore store;

        public ListRepository(IDocumentStore store)
        {
            this.store = store;
        }

        public ListCollection Lists()
        {
            using (var session = store.OpenSession())
            {
                var list =  session.Load<ListCollection>("system/lists");
                if (list == null)
                {
                    return new ListCollection();
                }
                return list;
            }
        }

        public void Add(ListDescriptor listDescriptor)
        {
            var listCollection = Lists();
            listCollection.Lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection,"system/lists");
            }

        }

        public void Remove(string id)
        {
            var listCollection = Lists();
            listCollection.Lists.RemoveAll(x => x.Id == id);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection, "system/lists");
            }
        }

        public void Update(ListDescriptor listDescriptor)
        {
            var listCollection = Lists();
            listCollection.Lists.RemoveAll(x => x.Id == listDescriptor.Id);
            listCollection.Lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection, "system/lists");
            }
        }
    }

    public class ListCollection
    {
        public List<ListDescriptor> Lists { get; set; }

        public ListCollection()
        {
            Lists = new List<ListDescriptor>();
        }
    }
}