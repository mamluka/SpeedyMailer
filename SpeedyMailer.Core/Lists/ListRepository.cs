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

        public List<ListDescriptor> Lists()
        {
            using (var session = store.OpenSession())
            {
                var list =  session.Load<List<ListDescriptor>>("system/lists");
                if (list == null)
                {
                    return new List<ListDescriptor>();
                }
                return list;
            }
        }

        public void Add(ListDescriptor listDescriptor)
        {
            var lists = Lists();
            lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(lists,"system/lists");
            }

        }

        public void Remove(string id)
        {
            var lists = Lists();
            lists.RemoveAll(x => x.Id == id);
            using (var session = store.OpenSession())
            {
                session.Store(lists, "system/lists");
            }
        }

        public void Update(ListDescriptor listDescriptor)
        {
            var lists = Lists();
            lists.RemoveAll(x => x.Id == listDescriptor.Id);
            lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(lists, "system/lists");
            }
        }
    }
}